using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
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
    private WaveInEvent _waveIn;
    private MemoryStream _audioStream;
    private WaveFileWriter _waveWriter;
    
    private SpeechSynthesizer _synthesizer;
    private bool _isListening = false;
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
            // Initialize Whisper.net
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
            AddMessage("System", "Whisper AI engine failed to initialize or model is missing.");
            return;
        }

        try
        {
            if (!_isListening)
            {
                // Start Recording
                _isListening = true;
                MicIcon.Visibility = Visibility.Collapsed;
                DotPulsePanel.Visibility = Visibility.Visible;
                _pulseAnimation.Begin();
                
                InputBox.Text = "Listening (Click Mic again to stop)...";
                InputBox.IsReadOnly = true;
                
                _audioStream = new MemoryStream();
                _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
                _waveWriter = new WaveFileWriter(_audioStream, _waveIn.WaveFormat);
                
                _waveIn.DataAvailable += (s, ev) => _waveWriter.Write(ev.Buffer, 0, ev.BytesRecorded);
                _waveIn.StartRecording();
            }
            else
            {
                // Stop and Transcribe
                _isListening = false;
                _pulseAnimation.Stop();
                DotPulsePanel.Visibility = Visibility.Collapsed;
                MicIcon.Visibility = Visibility.Visible;
                
                _waveIn?.StopRecording();
                _waveWriter?.Flush();
                _audioStream.Position = 0;
                
                InputBox.Text = "Transcribing with Whisper...";
                
                Task.Run(async () => 
                {
                    string fullText = "";
                    try
                    {
                        await foreach(var result in _whisperProcessor.ProcessAsync(_audioStream))
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
                        _waveWriter?.Dispose();
                        _waveIn?.Dispose();
                        _audioStream?.Dispose();
                    }
                    
                    string finalTrimmedText = fullText.Replace("[BLANK_AUDIO]", "").Replace("(blank audio)", "").Replace("[Silence]", "").Replace("[SILENCE]", "").Trim();
                    // Filter out common Whisper hallucination triggers if text is basically empty
                    if (finalTrimmedText.Length < 3 || finalTrimmedText.ToLower().Contains("subtitles by")) finalTrimmedText = "";
                    
                    Dispatcher.Invoke(() => 
                    {
                        InputBox.IsReadOnly = false;
                        if (!string.IsNullOrWhiteSpace(finalTrimmedText))
                        {
                            InputBox.Text = finalTrimmedText;
                            ProcessInput(finalTrimmedText);
                        }
                        else
                        {
                            InputBox.Text = "";
                            AddMessage("System", "Could not hear any speech clearly.");
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
            AddMessage("System", $"Microphone error: {ex.Message}");
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
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 0 
            };
            
            _model = LLamaWeights.LoadFromFile(parameters);
            _context = _model.CreateContext(parameters);
            _executor = new InteractiveExecutor(_context);
            
            _session = new ChatSession(_executor);
            string systemPrompt = "You are Kivo, a smart Windows AI assistant. IMPORTANT: ONLY output an XML action block IF the user explicitly asks you to perform a task on their computer. If the user just says hello or asks a question, reply with normal text and DO NOT output XML. Allowed XML formats:\n<action>open_app</action><app>code</app><args>path/to/folder</args>\n<action>create_folder</action><path>path/to/folder</path>\n<action>open_folder</action><path>path/to/folder</path>\n<action>search_web</action><query>query</query>\n<action>open_url</action><url>https://example.com</url>";
            _session.History.AddMessage(AuthorRole.System, systemPrompt);

            _isAiReady = true;
            Dispatcher.Invoke(() => Speak("Kivo is online and ready."));
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => AddMessage("System", $"Error loading AI: {ex.Message}"));
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
            ExpandIcon.Data = Geometry.Parse("M12,8.41L16.59,13L18,11.59L12,5.58L6,11.59L7.41,13L12,8.41Z"); // Up Arrow
        }
        else
        {
            this.Height = 85;
            ChatScrollViewer.Visibility = Visibility.Collapsed;
            ExpandIcon.Data = Geometry.Parse("M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z"); // Down Arrow
        }
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            ProcessInput(text);
        }
    }
    
    private async void ProcessInput(string text)
    {
        InputBox.Text = "";
        AddMessage("User", text);
        
        if (!_isAiReady)
        {
            AddMessage("Kivo", "Still loading the brain... Please wait a moment.");
            return;
        }

        InputBox.IsEnabled = false;

        try
        {
            string response = "";
            var replyBox = AddMessage("Kivo", "");
            
            var inferenceParams = new InferenceParams() 
            { 
                MaxTokens = 256, 
                AntiPrompts = new List<string> { "<|eot_id|>", "<|im_end|>", "User:", "\nUser:", "user\n" } 
            };

            await foreach (var token in _session.ChatAsync(
                               new ChatHistory.Message(AuthorRole.User, text), 
                               inferenceParams))
            {
                response += token;
                replyBox.Text = response;
                ChatScrollViewer.ScrollToEnd();
            }

            // Scrub XML from UI
            string cleanText = Regex.Replace(response, @"<action>.*?</action>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<app>.*?</app>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<args>.*?</args>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<path>.*?</path>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<query>.*?</query>", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"<url>.*?</url>", "", RegexOptions.Singleline).Trim();
            if (cleanText.StartsWith("Output:")) cleanText = cleanText.Substring(7).Trim();
            if (cleanText.StartsWith("You:")) cleanText = cleanText.Substring(4).Trim();
            if (cleanText.EndsWith("User:")) cleanText = cleanText.Substring(0, cleanText.Length - 5).Trim();
            
            bool hasAction = response.Contains("<action>");
            
            if (string.IsNullOrWhiteSpace(cleanText))
            {
                replyBox.Text = "Executing action...";
            }
            else
            {
                replyBox.Text = cleanText;
                if (!hasAction) Speak(cleanText); 
            }

            // Check for XML actions
            if (hasAction)
            {
                var actionMatch = Regex.Match(response, @"<action>(.*?)</action>", RegexOptions.Singleline);
                if (actionMatch.Success)
                {
                    string action = actionMatch.Groups[1].Value.Trim();
                    string param1 = Regex.Match(response, @"<path>(.*?)</path>", RegexOptions.Singleline).Groups[1].Value?.Trim() ?? "";
                    if (string.IsNullOrEmpty(param1)) param1 = Regex.Match(response, @"<app>(.*?)</app>", RegexOptions.Singleline).Groups[1].Value?.Trim() ?? "";
                    if (string.IsNullOrEmpty(param1)) param1 = Regex.Match(response, @"<query>(.*?)</query>", RegexOptions.Singleline).Groups[1].Value?.Trim() ?? "";
                    if (string.IsNullOrEmpty(param1)) param1 = Regex.Match(response, @"<url>(.*?)</url>", RegexOptions.Singleline).Groups[1].Value?.Trim() ?? "";
                    
                    string param2 = Regex.Match(response, @"<args>(.*?)</args>", RegexOptions.Singleline).Groups[1].Value?.Trim() ?? "";

                    // Prevent hallucinations
                    if (param1 != "path/to/folder" && param1 != "query")
                    {
                        string displayParam = param1 + (string.IsNullOrEmpty(param2) ? "" : $" (Args: {param2})");
                        var permission = MessageBox.Show($"Kivo wants to execute the following action:\n\nAction: {action}\nParameter: {displayParam}\n\nDo you want to allow this?", "Kivo Security Layer", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        
                        if (permission == MessageBoxResult.Yes)
                        {
                            string result = ActionExecutor.Execute(action, param1, param2);
                            AddMessage("System", result);
                            Speak(result); 
                        }
                        else
                        {
                            AddMessage("System", "Action blocked by user security layer.");
                            Speak("Action cancelled.");
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
            InputBox.IsEnabled = true;
            InputBox.Focus();
        }
    }
    
    private TextBlock AddMessage(string sender, string message)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = sender == "User" ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Background = sender == "User" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B30078D7")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3333333"))
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