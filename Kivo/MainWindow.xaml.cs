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
            _whisperFactory = WhisperFactory.FromPath(@"d:\kivo\models\ggml-small.en.bin");
            _whisperProcessor = _whisperFactory.CreateBuilder().WithLanguage("en").Build();
            
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Speech init error: {ex.Message}");
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
            
            _session = new ChatSession(_executor);
            string systemPrompt = @"You are Kivo, a helpful Windows desktop assistant. Keep replies short and direct.

RULES:
- If the user greets you or asks a question, reply with plain text. Do NOT output XML.
- ONLY use XML action tags when the user explicitly asks to DO something on their computer.

Available actions:
<action>open_app</action><app>appname</app><args>optional args</args>
<action>create_folder</action><path>C:\full\path</path>
<action>open_folder</action><path>C:\full\path</path>
<action>search_web</action><query>search terms</query>
<action>open_url</action><url>https://example.com</url>

Examples:
User: Hello -> Hi! How can I help?
User: Open VS Code -> <action>open_app</action><app>code</app>
User: Search for cats -> <action>search_web</action><query>cats</query>";

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
            string response = "";
            var replyBox = AddMessage("Kivo", "");
            
            var inferenceParams = new InferenceParams() 
            { 
                MaxTokens = 256, 
                AntiPrompts = new List<string> { "<|eot_id|>", "<|im_end|>", "\nUser:", "User:" } 
            };

            // Fix ChatSession crash: ensure history alternates User/Assistant
            var lastMsg = _session.History.Messages.LastOrDefault();
            if (lastMsg != null && lastMsg.AuthorRole == AuthorRole.User)
            {
                _session.History.AddMessage(AuthorRole.Assistant, "(ok)");
            }

            await foreach (var token in _session.ChatAsync(
                               new ChatHistory.Message(AuthorRole.User, text), 
                               inferenceParams))
            {
                response += token;
                replyBox.Text = response;
                ChatScrollViewer.ScrollToEnd();
            }

            // Clean response for display
            string cleanText = response;
            cleanText = Regex.Replace(cleanText, @"<action>.*?</action>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<app>.*?</app>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<args>.*?</args>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<path>.*?</path>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<query>.*?</query>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<url>.*?</url>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<\|.*?\|>", "", RegexOptions.Singleline); // Strip any leaked special tokens
            cleanText = cleanText.Trim();
            if (cleanText.EndsWith("User:")) cleanText = cleanText[..^5].Trim();
            
            bool hasAction = response.Contains("<action>");
            
            if (hasAction && string.IsNullOrWhiteSpace(cleanText))
            {
                replyBox.Text = "On it...";
            }
            else if (!string.IsNullOrWhiteSpace(cleanText))
            {
                replyBox.Text = cleanText;
                if (!hasAction) Speak(cleanText); 
            }
            else
            {
                replyBox.Text = "Done.";
            }

            // Parse and execute XML actions
            if (hasAction)
            {
                var actionMatch = Regex.Match(response, @"<action>(.*?)</action>", RegexOptions.Singleline);
                if (actionMatch.Success)
                {
                    string action = actionMatch.Groups[1].Value.Trim();
                    
                    // Extract params in priority order
                    string param1 = ExtractTag(response, "path");
                    if (string.IsNullOrEmpty(param1)) param1 = ExtractTag(response, "app");
                    if (string.IsNullOrEmpty(param1)) param1 = ExtractTag(response, "query");
                    if (string.IsNullOrEmpty(param1)) param1 = ExtractTag(response, "url");
                    
                    string param2 = ExtractTag(response, "args");

                    // Skip template placeholders
                    if (IsPlaceholder(param1))
                    {
                        AddMessage("System", "Kivo couldn't determine what to do. Try being more specific.");
                    }
                    else if (!string.IsNullOrEmpty(param1))
                    {
                        string displayParam = param1 + (string.IsNullOrEmpty(param2) ? "" : $" ({param2})");
                        var permission = MessageBox.Show(
                            $"Kivo wants to:\n\nAction: {action}\nTarget: {displayParam}\n\nAllow?", 
                            "Kivo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (permission == MessageBoxResult.Yes)
                        {
                            string result = ActionExecutor.Execute(action, param1, param2);
                            AddMessage("System", result);
                            Speak(result); 
                        }
                        else
                        {
                            AddMessage("System", "Action cancelled.");
                        }
                    }
                }
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