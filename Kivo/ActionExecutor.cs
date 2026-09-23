using System;
using System.Diagnostics;
using System.IO;

namespace Kivo
{
    public static class ActionExecutor
    {
        public static string Execute(string action, string param1 = null, string param2 = null)
        {
            try
            {
                string act = action.ToLower();

                // 1. Create Folder
                if (act.Contains("create") && act.Contains("folder") || act == "mkdir")
                {
                    if (string.IsNullOrEmpty(param1)) return "Error: Path not provided.";
                    param1 = param1.Replace('/', '\\');
                    if (param1.StartsWith("\\") && param1.Length >= 3 && param1[2] == ':') param1 = param1.Substring(1);
                    Directory.CreateDirectory(param1);
                    Process.Start(new ProcessStartInfo { FileName = param1, UseShellExecute = true });
                    return $"Successfully created and opened folder at {param1}";
                }
                
                // 2. Open Folder
                if (act.Contains("folder") || act.Contains("directory"))
                {
                    if (string.IsNullOrEmpty(param1)) return "Error: Path not provided.";
                    param1 = param1.Replace('/', '\\');
                    if (param1.StartsWith("\\") && param1.Length >= 3 && param1[2] == ':') param1 = param1.Substring(1);
                    Process.Start(new ProcessStartInfo { FileName = param1, UseShellExecute = true });
                    return $"Opened folder {param1}.";
                }

                // 3. Open App
                if (act.Contains("app") || act == "open_program" || act == "run_command")
                {
                    if (string.IsNullOrEmpty(param1)) return "Error: App name or path not provided.";
                    Process.Start(new ProcessStartInfo { FileName = param1, Arguments = param2 ?? "", UseShellExecute = true });
                    return $"Opened {param1}.";
                }

                // 4. Web Search / Open URL (Matches "open_web_browser", "search_web", etc.)
                if (act.Contains("search") || act.Contains("web") || act.Contains("url") || act.Contains("browser") || act.Contains("chrome") || act.Contains("edge") || act.Contains("google") || act.Contains("internet"))
                {
                    if (string.IsNullOrEmpty(param1)) return "Error: Search query not provided.";
                    bool isUrl = param1.StartsWith("http") || param1.Contains("www.") || param1.Contains(".com") || param1.Contains(".org");
                    string target = isUrl ? (param1.StartsWith("http") ? param1 : "https://" + param1) : $"https://www.google.com/search?q={Uri.EscapeDataString(param1)}";
                    
                    Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
                    return $"Opened browser for '{param1}'.";
                }

                // 5. Ultimate Fallback: If it hallucinates a totally random action name but still provides a parameter
                if (!string.IsNullOrEmpty(param1))
                {
                    bool isUrl = param1.StartsWith("http") || param1.Contains("www.") || param1.Contains(".com") || param1.Contains(".org");
                    string target = isUrl ? (param1.StartsWith("http") ? param1 : "https://" + param1) : $"https://www.google.com/search?q={Uri.EscapeDataString(param1)}";
                    Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
                    return $"Executed fallback web handler for '{param1}'.";
                }

                return $"Unknown action: {action}";
            }
            catch (Exception ex)
            {
                return $"Error executing {action}: {ex.Message}";
            }
        }
    }
}
