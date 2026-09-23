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
                switch (action.ToLower())
                {
                    case "create_folder":
                        if (string.IsNullOrEmpty(param1)) return "Error: Path not provided.";
                        param1 = param1.Replace('/', '\\');
                        if (param1.StartsWith("\\") && param1.Length >= 3 && param1[2] == ':') param1 = param1.Substring(1);
                        Directory.CreateDirectory(param1);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = param1,
                            UseShellExecute = true
                        });
                        return $"Successfully created and opened folder at {param1}";

                    case "open_folder":
                        if (string.IsNullOrEmpty(param1)) return "Error: Path not provided.";
                        param1 = param1.Replace('/', '\\');
                        if (param1.StartsWith("\\") && param1.Length >= 3 && param1[2] == ':') param1 = param1.Substring(1);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = param1,
                            UseShellExecute = true
                        });
                        return $"Opened folder {param1}.";

                    case "open_app":
                        if (string.IsNullOrEmpty(param1)) return "Error: App name or path not provided.";
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = param1,
                            Arguments = param2 ?? "",
                            UseShellExecute = true
                        });
                        return $"Opened {param1}.";

                    case "search_web":
                        if (string.IsNullOrEmpty(param1)) return "Error: Search query not provided.";
                        var query = Uri.EscapeDataString(param1);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = $"https://www.google.com/search?q={query}",
                            UseShellExecute = true
                        });
                        return $"Searched the web for '{param1}'.";
                    
                    case "open_url":
                    case "open_webpage":
                    case "open_website":
                    case "open_browser":
                        if (string.IsNullOrEmpty(param1)) return "Error: URL not provided.";
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = param1.StartsWith("http") ? param1 : $"https://www.google.com/search?q={Uri.EscapeDataString(param1)}",
                            UseShellExecute = true
                        });
                        return $"Opened {param1}.";

                    default:
                        return $"Unknown action: {action}";
                }
            }
            catch (Exception ex)
            {
                return $"Error executing {action}: {ex.Message}";
            }
        }
    }
}
