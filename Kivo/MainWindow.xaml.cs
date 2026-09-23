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
using System.Speech.Recognition;

namespace Kivo;

public partial class MainWindow : Window
{
    private LLamaWeights _model;
    private LLamaContext _context;
    private InteractiveExecutor _executor;
    private ChatSession _session;
    private bool _isAiReady = false;
    
    // Speech Recognition
    private SpeechRecognitionEngine _recognizer;
    
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
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
            
            _recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            _recognizer.LoadGrammar(new DictationGrammar());
            _recognizer.SetInputToDefaultAudioDevice();
            
            _recognizer.SpeechRecognized += (s, e) => 
            {
                if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
                {
                    Dispatcher.Invoke(() => 
                    {
                        InputBox.Text = e.Result.Text;
                        ProcessInput(e.Result.Text);
                        
                        // Stop listening after a command is spoken
                        _isListening = false;
                        _pulseAnimation.Stop();
                        DotPulsePanel.Visibility = Visibility.Collapsed;
                        MicIcon.Visibility = Visibility.Visible;
                        InputBox.IsReadOnly = false;
                        _recognizer.RecognizeAsyncCancel();
                    });
                }
            };
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
        if (_recognizer == null)
        {
            AddMessage("System", "Speech engine failed to initialize.");
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
                
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                // Stop and Transcribe
                _isListening = false;
                _pulseAnimation.Stop();
                DotPulsePanel.Visibility = Visibility.Collapsed;
                MicIcon.Visibility = Visibility.Visible;
                
                InputBox.IsReadOnly = false;
                InputBox.Text = "";
                
                _recognizer.RecognizeAsyncCancel();
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
            string modelPath = @"d:\kivo\models\qwen2.5-0.5b-instruct-q4_k_m.gguf";
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 0 
            };
            
            _model = LLamaWeights.LoadFromFile(parameters);
            _context = _model.CreateContext(parameters);
            _executor = new InteractiveExecutor(_context);
            
            _session = new ChatSession(_executor);
            string systemPrompt = "You are Kivo, a smart Windows AI assistant. IMPORTANT: ONLY output a JSON action block IF the user explicitly asks you to perform a task. If the user just says hello or asks a question, reply with normal text and DO NOT output JSON. Do NOT hallucinate code. Allowed JSON format:\n{\"action\": \"open_app\", \"app\": \"code\", \"args\": \"path/to/folder\"}\n{\"action\": \"create_folder\", \"path\": \"path/to/folder\"}\n{\"action\": \"open_folder\", \"path\": \"path/to/folder\"}\n{\"action\": \"search_web\", \"query\": \"query\"}";
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
                AntiPrompts = new List<string> { "<|im_end|>", "<|im_start|>", "user\n", "User:", "\nUser:" } 
            };

            await foreach (var token in _session.ChatAsync(
                               new ChatHistory.Message(AuthorRole.User, text + "\n<|im_start|>assistant\n"), 
                               inferenceParams))
            {
                response += token;
                replyBox.Text = response;
                ChatScrollViewer.ScrollToEnd();
            }

            // Scrub JSON from UI
            string cleanText = Regex.Replace(response, @"```json.*?```", "", RegexOptions.Singleline);
            cleanText = Regex.Replace(cleanText, @"\{.*?\}", "", RegexOptions.Singleline).Trim();
            if (cleanText.StartsWith("Output:")) cleanText = cleanText.Substring(7).Trim();
            if (cleanText.StartsWith("You:")) cleanText = cleanText.Substring(4).Trim();
            
            bool hasJsonAction = Regex.IsMatch(response, @"\{.*?\}", RegexOptions.Singleline);
            
            if (string.IsNullOrWhiteSpace(cleanText))
            {
                replyBox.Text = "Executing action...";
            }
            else
            {
                replyBox.Text = cleanText;
                if (!hasJsonAction) Speak(cleanText); 
            }

            // Check for JSON actions using Regex
            var matches = Regex.Matches(response, @"\{[^{}]*\}", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(match.Value);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("action", out var actionProp))
                    {
                        string action = actionProp.GetString();
                        string param1 = root.TryGetProperty("path", out var pathProp) ? pathProp.GetString() : 
                                        root.TryGetProperty("app", out var appProp) ? appProp.GetString() : 
                                        root.TryGetProperty("query", out var queryProp) ? queryProp.GetString() : null;
                        string param2 = root.TryGetProperty("args", out var argsProp) ? argsProp.GetString() : null;

                        // Prevent hallucinations
                        if (param1 == "absolute_path_here" || param1 == "path/to/folder" || param1 == "empty_folder_path") continue;

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
                catch (JsonException) { /* Not a valid JSON action, ignore */ }
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