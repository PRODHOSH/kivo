using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Speech.Synthesis;
using System.Windows.Media.Animation;
using LLama.Common;
using LLama;
using Whisper.net;
using NAudio.Wave;

namespace Kivo;

public partial class MainWindow : Window
{
    private LLamaWeights _model;
    private LLamaContext _context;
    private InteractiveExecutor _executor;
    private StatelessExecutor _classifier;  // stateless intent classifier
    private ChatSession _session;
    private bool _isAiReady = false;
    
    // Whisper
    private WhisperFactory _whisperFactory;
    private WhisperProcessor _whisperProcessor;
    private WaveInEvent? _waveIn;
    private MemoryStream? _audioStream;
    private WaveFileWriter? _waveWriter;
    
    private SpeechSynthesizer _synthesizer;
    private bool _isListening = false;
    private bool _isProcessing = false;
    private Storyboard _pulseAnimation;

    public MainWindow()
    {
        InitializeComponent();
        _pulseAnimation = (Storyboard)this.Resources["DotPulseAnimation"];
        Task.Run(InitializeAiAsync);
        Task.Run(InitializeSpeechAsync);
    }
    
    private async Task InitializeSpeechAsync()
    {
        try
        {
            // Try ggml-small first, fall back to ggml-tiny
            string[] whisperPaths = {
                @"d:\kivo\models\ggml-small.en.bin",
                @"d:\kivo\models\ggml-tiny.en.bin"
            };
            string? whisperPath = whisperPaths.FirstOrDefault(File.Exists);
            if (whisperPath == null)
            {
                Dispatcher.Invoke(() => AddMessage("System", "Whisper model not found. Voice input disabled."));
                return;
            }
            _whisperFactory = WhisperFactory.FromPath(whisperPath);
            _whisperProcessor = _whisperFactory.CreateBuilder().WithLanguage("en").Build();
            
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => AddMessage("System", $"Voice init error: {ex.Message}"));
        }
    }
    
    private void Speak(string text)
    {
        if (_synthesizer != null && !string.IsNullOrWhiteSpace(text))
        {
            _synthesizer.SpeakAsyncCancelAll();
            _synthesizer.SpeakAsync(text);
        }
    }
    
    private void MicButton_Click(object sender, RoutedEventArgs e)
    {
        if (_whisperProcessor == null)
        {
            AddMessage("System", "Whisper AI engine not loaded. Check models folder.");
            return;
        }

        if (_isProcessing) return; // Don't allow mic during AI processing

        try
        {
            if (!_isListening)
            {
                _isListening = true;
                MicIcon.Visibility = Visibility.Collapsed;
                DotPulsePanel.Visibility = Visibility.Visible;
                _pulseAnimation.Begin();
                
                InputBox.Text = "Listening...";
                InputBox.IsReadOnly = true;
                
                _audioStream = new MemoryStream();
                _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
                _waveWriter = new WaveFileWriter(_audioStream, _waveIn.WaveFormat);
                
                _waveIn.DataAvailable += (s, ev) => _waveWriter.Write(ev.Buffer, 0, ev.BytesRecorded);
                _waveIn.StartRecording();
            }
            else
            {
                _isListening = false;
                _pulseAnimation.Stop();
                DotPulsePanel.Visibility = Visibility.Collapsed;
                MicIcon.Visibility = Visibility.Visible;
                
                _waveIn?.StopRecording();
                _waveWriter?.Flush();

                if (_audioStream == null || _audioStream.Length == 0)
                {
                    InputBox.IsReadOnly = false;
                    InputBox.Text = "";
                    AddMessage("System", "No audio captured.");
                    return;
                }

                _audioStream.Position = 0;
                InputBox.Text = "Transcribing...";

                // Capture references before async work
                var stream = _audioStream;
                var writer = _waveWriter;
                var recorder = _waveIn;
                
                Task.Run(async () => 
                {
                    string fullText = "";
                    try
                    {
                        await foreach(var result in _whisperProcessor.ProcessAsync(stream))
                        {
                            fullText += result.Text;
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() => AddMessage("System", $"Whisper Error: {ex.Message}"));
                    }
                    finally
                    {
                        writer?.Dispose();
                        recorder?.Dispose();
                        stream?.Dispose();
                    }
                    
                    // Clean up Whisper artifacts
                    string cleaned = fullText
                        .Replace("[BLANK_AUDIO]", "").Replace("(blank audio)", "")
                        .Replace("[Silence]", "").Replace("[SILENCE]", "")
                        .Replace("[Music]", "").Replace("[MUSIC]", "")
                        .Trim();
                    
                    // Filter hallucination triggers
                    if (cleaned.Length < 3 
                        || cleaned.ToLower().Contains("subtitles by") 
                        || cleaned.ToLower().Contains("thank you for watching"))
                    {
                        cleaned = "";
                    }
                    
                    Dispatcher.Invoke(() => 
                    {
                        InputBox.IsReadOnly = false;
                        if (!string.IsNullOrWhiteSpace(cleaned))
                        {
                            InputBox.Text = cleaned;
                            ProcessInput(cleaned);
                        }
                        else
                        {
                            InputBox.Text = "";
                            AddMessage("System", "Couldn't hear anything. Try again.");
                        }
                    });
                });
            }
        }
        catch (Exception ex)
        {
            _isListening = false;
            _pulseAnimation.Stop();
            DotPulsePanel.Visibility = Visibility.Collapsed;
            MicIcon.Visibility = Visibility.Visible;
            InputBox.IsReadOnly = false;
            InputBox.Text = "";
            AddMessage("System", $"Mic error: {ex.Message}");
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    private async Task InitializeAiAsync()
    {
        try
        {
            string modelPath = @"d:\kivo\models\Llama-3.2-1B-Instruct.gguf";
            if (!File.Exists(modelPath))
            {
                Dispatcher.Invoke(() => AddMessage("System", "AI model not found. Download Llama-3.2-1B-Instruct.gguf to models folder."));
                return;
            }

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 2048,
                GpuLayerCount = 0 
            };
            
            _model = LLamaWeights.LoadFromFile(parameters);
            _context = _model.CreateContext(parameters);
            _executor = new InteractiveExecutor(_context);
            
            // Classifier: stateless executor for intent detection only
            var classifierContext = _model.CreateContext(parameters);
            _classifier = new StatelessExecutor(_model, parameters);

            // Chat session for conversational replies ONLY
            _session = new ChatSession(_executor);
            string systemPrompt =
                "You are Kivo, a voice assistant. " +
                "STRICT RULES - follow exactly:\n" +
                "1. Reply in 1-2 short sentences MAXIMUM.\n" +
                "2. For commands (open, search, create, go to): reply ONLY with 'On it!' or 'Sure!' or 'Done!'. Nothing else.\n" +
                "3. NEVER say what is or isn't installed on the computer.\n" +
                "4. NEVER report weather, news, time, or any real-world data.\n" +
                "5. NEVER pretend you ran a command or checked anything.\n" +
                "6. NEVER say the user is not logged in or needs to install something.\n" +
                "7. For greetings: reply warmly in 1 sentence.\n" +
                "8. For questions you cannot answer: say 'I'm not sure about that.'";


            _session.History.AddMessage(AuthorRole.System, systemPrompt);

            _isAiReady = true;
            Dispatcher.Invoke(() => Speak("Kivo is ready."));
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => AddMessage("System", $"AI load error: {ex.Message}"));
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.Height <= 85)
        {
            this.Height = 450;
            ChatScrollViewer.Visibility = Visibility.Visible;
            ExpandIcon.Data = Geometry.Parse("M12,8.41L16.59,13L18,11.59L12,5.58L6,11.59L7.41,13L12,8.41Z");
        }
        else
        {
            this.Height = 85;
            ChatScrollViewer.Visibility = Visibility.Collapsed;
            ExpandIcon.Data = Geometry.Parse("M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z");
        }
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text) || _isProcessing) return;
            ProcessInput(text);
        }
    }
    
    private async void ProcessInput(string text)
    {
        InputBox.Text = "";
        AddMessage("User", text);
        
        if (!_isAiReady)
        {
            AddMessage("Kivo", "Still loading the brain... Please wait.");
            return;
        }

        _isProcessing = true;
        InputBox.IsEnabled = false;

        try
        {
            var replyBox = AddMessage("Kivo", "Thinking...");

            // ── PRE-CHECK: Reliable pattern detection for things the 1B model struggles with ──
            string? preDetectedAction = null;
            string? preDetectedParam = null;

            string textLow = text.ToLower();

            // "go to settings > storage" / "open storage settings" / "open settings and go to storage"
            var settingsKeywords = new[] { "bluetooth", "wifi", "wi-fi", "storage", "display", "sound", "network", "battery",
                "update", "privacy", "firewall", "startup", "taskbar", "wallpaper", "background", "accounts",
                "notifications", "power", "apps", "language", "region", "personalization", "accessibility",
                "lock screen", "themes", "fonts", "keyboard", "mouse", "touchpad", "usb", "gaming",
                "developer mode", "clipboard", "time", "date", "recovery", "backup", "activation" };

            bool mentionsSettings = textLow.Contains("setting") || textLow.Contains("settings");
            if (mentionsSettings)
            {
                // Find which settings page they want
                string settingsPage = "";
                foreach (var kw in settingsKeywords)
                    if (textLow.Contains(kw)) { settingsPage = kw; break; }
                preDetectedAction = "open_settings";
                preDetectedParam = settingsPage; // empty = main settings page
            }
            // URL detection: "go to www.pw.com", "open github.com"
            else
            {
                var urlInText = Regex.Match(text, @"(?:go\s+to|navigate\s+to|visit|open)\s+((?:https?://|www\.)\S+|\S+\.(?:com|org|io|net|co|in|uk|gov|edu)\S*)", RegexOptions.IgnoreCase);
                if (urlInText.Success)
                {
                    string url = urlInText.Groups[1].Value.Trim().TrimEnd('.', ',');
                    if (!url.StartsWith("http")) url = "https://" + url;
                    preDetectedAction = "open_url";
                    preDetectedParam = url;
                }
                else
                {
                    // Scan FULL text for known app names — handles:
                    // "open X", "launch X", "can you open X", "please open X", "hey open X and..."
                    // Extract what comes after any open/launch/start/run verb
                    var openVerbMatch = Regex.Match(text,
                        @"(?:can\s+you\s+|please\s+|could\s+you\s+|hey\s+)?(?:open|launch|start|run)\s+(.+?)(?:\s+(?:for\s+me|please|now|and\s+tell|and\s+show).*)?$",
                        RegexOptions.IgnoreCase);
                    if (openVerbMatch.Success)
                    {
                        string appCandidate = openVerbMatch.Groups[1].Value.Trim();
                        string appClean = Regex.Replace(appCandidate, @"\s*(app|application|program|software)$", "", RegexOptions.IgnoreCase).Trim();
                        string? resolved = ActionExecutor.ResolveAppName(appClean);
                        if (resolved != null)
                        {
                            preDetectedAction = "open_app";
                            preDetectedParam = appClean;
                        }
                    }

                    // Detect question-style search: "what's the weather in Chennai?", "who is X?"
                    if (preDetectedAction == null)
                    {
                        var questionMatch = Regex.Match(text,
                            @"^(?:what(?:'s|\s+is)|who\s+is|how\s+(?:to|do)|when\s+(?:is|was)|where\s+is|why\s+is|search\s+for)\s+(.+?)\??",
                            RegexOptions.IgnoreCase);
                        if (questionMatch.Success)
                        {
                            preDetectedAction = "search_web";
                            preDetectedParam = questionMatch.Groups[1].Value.Trim().TrimEnd('?', '.');
                        }
                    }
                }
            }

            // ── STEP 1: LLM Intent classifier ─────────────────────────────────
            string intentXml = "";

            if (preDetectedAction == null)
            {
                // Only run LLM classifier when we don't already know the intent
                string classifierPrompt = string.Concat(
                    "<|begin_of_text|><|start_header_id|>system<|end_header_id|>\n",
                    "Classify the user command. Output XML or 'none'.\n",
                    "search for X -> <action>search_web</action><query>X</query>\n",
                    "google X -> <action>search_web</action><query>X</query>\n",
                    "open notepad -> <action>open_app</action><app>notepad</app>\n",
                    "open docker -> <action>open_app</action><app>docker</app>\n",
                    "open vs code -> <action>open_app</action><app>vs code</app>\n",
                    "open spotify -> <action>open_app</action><app>spotify</app>\n",
                    "create folder myfiles -> <action>create_folder</action><path>myfiles</path>\n",
                    "open downloads folder -> <action>open_folder</action><path>downloads</path>\n",
                    "hello / questions -> none\n",
                    "<|eot_id|><|start_header_id|>user<|end_header_id|>\n",
                    text,
                    "<|eot_id|><|start_header_id|>assistant<|end_header_id|>\n"
                );

                var classifierParams = new InferenceParams { MaxTokens = 60, AntiPrompts = new List<string> { "<|eot_id|>", "\n\n", "User:", "\nKivo:" } };
                await foreach (var tok in _classifier.InferAsync(classifierPrompt, classifierParams))
                    intentXml += tok;
                intentXml = intentXml.Trim();
            }

            // ── STEP 2: Chat session for conversational reply ────────────────
            string chatReply = "";
            var chatParams = new InferenceParams { MaxTokens = 128, AntiPrompts = new List<string> { "<|eot_id|>", "<|im_end|>", "\nUser:", "User:" } };

            var lastMsg = _session.History.Messages.LastOrDefault();
            if (lastMsg != null && lastMsg.AuthorRole == AuthorRole.User)
                _session.History.AddMessage(AuthorRole.Assistant, "(ok)");

            await foreach (var token in _session.ChatAsync(new ChatHistory.Message(AuthorRole.User, text), chatParams))
            {
                chatReply += token;
                // Strip leaked tokens during streaming
                string display = Regex.Replace(chatReply, @"<\|.*?\|>", "").Trim();
                replyBox.Text = string.IsNullOrWhiteSpace(display) ? "Thinking..." : display;
                ChatScrollViewer.ScrollToEnd();
            }

            // Clean final chat reply — detect and suppress hallucinated placeholder text
            chatReply = Regex.Replace(chatReply, @"<\|.*?\|>", "").Trim();
            if (chatReply.EndsWith("User:")) chatReply = chatReply[..^5].Trim();
            // If the reply contains [placeholder] brackets, the model is hallucinating facts — suppress it
            bool hasPlaceholder = Regex.IsMatch(chatReply, @"\[.{1,30}\]");
            replyBox.Text = (string.IsNullOrWhiteSpace(chatReply) || hasPlaceholder) ? "Got it!" : chatReply;

            // ── STEP 3: Execute intent ───────────────────────────────────────
            string finalAction = "";
            string finalParam = "";
            string finalParam2 = "";

            if (preDetectedAction != null && preDetectedParam != null)
            {
                // Route ALL pre-detected actions through the permission layer
                finalAction = preDetectedAction;
                finalParam = preDetectedParam ?? "";
            }
            else if (!string.IsNullOrWhiteSpace(intentXml) && intentXml != "none" && intentXml.Contains("<action>"))
            {
                finalAction = ExtractTag(intentXml, "action");
                finalParam = ExtractTag(intentXml, "query");
                if (string.IsNullOrEmpty(finalParam)) finalParam = ExtractTag(intentXml, "url");
                if (string.IsNullOrEmpty(finalParam)) finalParam = ExtractTag(intentXml, "app");
                if (string.IsNullOrEmpty(finalParam)) finalParam = ExtractTag(intentXml, "path");
                finalParam2 = ExtractTag(intentXml, "args");
            }

            if (!string.IsNullOrEmpty(finalAction) && !string.IsNullOrEmpty(finalParam) && !IsPlaceholder(finalParam))
            {
                AskPermissionAndExecute(finalAction, finalParam, finalParam2);
            }
        }
        catch (Exception ex)
        {
            AddMessage("System", $"Error: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            InputBox.IsEnabled = true;
            InputBox.Focus();
        }
    }

    private static string ExtractTag(string text, string tag)
    {
        var match = Regex.Match(text, $@"<{tag}>(.*?)</{tag}>", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static bool IsPlaceholder(string value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        string lower = value.ToLower();
        return lower == "path/to/folder" || lower == "query" || lower == "appname" 
            || lower == "https://example.com" || lower == "search terms"
            || lower == "optional args" || lower.Contains("example");
    }

    private void AskPermissionAndExecute(string action, string param1, string? param2)
    {
        // Build a human-friendly permission message
        string friendlyDesc = BuildFriendlyDescription(action, param1);
        
        // Destructive actions get a warning icon
        bool isDestructive = action.Contains("shutdown") || action.Contains("restart") 
                          || action.Contains("lock") || action.Contains("delete");
        
        var icon   = isDestructive ? MessageBoxImage.Warning : MessageBoxImage.Question;
        var result = MessageBox.Show(
            $"{friendlyDesc}\n\nAllow Kivo to do this?",
            "Kivo — Permission Required",
            MessageBoxButton.YesNo,
            icon);

        if (result == MessageBoxResult.Yes)
        {
            // For settings, call OpenSettings directly so the Settings map is used
            string outcome = action == "open_settings"
                ? ActionExecutor.OpenSettings(param1)
                : ActionExecutor.Execute(action, param1, param2);

            AddMessage("System", outcome);
            Speak(outcome);
        }
        else
        {
            AddMessage("System", "Cancelled.");
        }
    }

    private static string BuildFriendlyDescription(string action, string param)
    {
        string act = action.ToLower().Trim();
        return act switch
        {
            "search_web"       => $"🔍 Search Google for: \"{param}\".",
            "open_url"         => $"🌐 Open website: {param}",
            "open_app"         => $"📂 Launch app: {param}",
            "open_folder"      => $"📁 Open folder: {param}",
            "create_folder"    => $"🗂 Create folder named: \"{param}\" on your Desktop.",
            "open_settings"    => string.IsNullOrWhiteSpace(param)
                                    ? "⚙️ Open Windows Settings."
                                    : $"⚙️ Open Windows Settings → {char.ToUpper(param[0]) + param[1..]}.",
            var s when s.Contains("shutdown")  => "⚠️ Shut down your computer in 10 seconds.",
            var s when s.Contains("restart")   => "⚠️ Restart your computer in 10 seconds.",
            var s when s.Contains("lock")      => "🔒 Lock your workstation.",
            var s when s.Contains("sleep")     => "💤 Put your computer to sleep.",
            var s when s.Contains("screenshot")=> "📸 Open the Snipping Tool to take a screenshot.",
            var s when s.Contains("volume")    => $"🔊 Change volume: {param}.",
            var s when s.Contains("mute")      => "🔇 Mute system volume.",
            _                  => $"Run: {action} → {param}"
        };
    }


    
    private TextBlock AddMessage(string sender, string message)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = sender == "User" ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Background = sender == "User" 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B30078D7")) 
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3333333"))
        };

        var textBlock = new TextBlock
        {
            Text = message,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        };

        border.Child = textBlock;
        ChatPanel.Children.Add(border);
        ChatScrollViewer.ScrollToEnd();
        
        return textBlock;
    }
}