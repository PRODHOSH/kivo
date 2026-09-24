using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Kivo
{
    public static class ActionExecutor
    {
        // ── Windows Settings URI map ──────────────────────────────────────────
        private static readonly Dictionary<string, string> SettingsMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["storage"]            = "ms-settings:storagesense",
            ["storage sense"]      = "ms-settings:storagesense",
            ["display"]            = "ms-settings:display",
            ["brightness"]         = "ms-settings:display",
            ["sound"]              = "ms-settings:sound",
            ["volume"]             = "ms-settings:sound",
            ["notifications"]      = "ms-settings:notifications",
            ["power"]              = "ms-settings:powersleep",
            ["power sleep"]        = "ms-settings:powersleep",
            ["battery"]            = "ms-settings:batterysaver",
            ["multitasking"]       = "ms-settings:multitasking",
            ["about"]              = "ms-settings:about",
            ["system"]             = "ms-settings:system",
            ["bluetooth"]          = "ms-settings:bluetooth",
            ["printers"]           = "ms-settings:printers",
            ["mouse"]              = "ms-settings:mousetouchpad",
            ["touchpad"]           = "ms-settings:mousetouchpad",
            ["keyboard"]           = "ms-settings:keyboard",
            ["usb"]                = "ms-settings:usb",
            ["autoplay"]           = "ms-settings:autoplay",
            ["network"]            = "ms-settings:network",
            ["wifi"]               = "ms-settings:network-wifi",
            ["wi-fi"]              = "ms-settings:network-wifi",
            ["ethernet"]           = "ms-settings:network-ethernet",
            ["vpn"]                = "ms-settings:network-vpn",
            ["proxy"]              = "ms-settings:network-proxy",
            ["airplane mode"]      = "ms-settings:network-airplanemode",
            ["hotspot"]            = "ms-settings:network-mobilehotspot",
            ["mobile hotspot"]     = "ms-settings:network-mobilehotspot",
            ["data usage"]         = "ms-settings:datausage",
            ["personalization"]    = "ms-settings:personalization",
            ["background"]         = "ms-settings:personalization-background",
            ["wallpaper"]          = "ms-settings:personalization-background",
            ["colors"]             = "ms-settings:colors",
            ["themes"]             = "ms-settings:themes",
            ["taskbar"]            = "ms-settings:taskbar",
            ["start menu"]         = "ms-settings:personalization-start",
            ["lock screen"]        = "ms-settings:lockscreen",
            ["fonts"]              = "ms-settings:fonts",
            ["accounts"]           = "ms-settings:accounts",
            ["email"]              = "ms-settings:emailandaccounts",
            ["sign-in options"]    = "ms-settings:signinoptions",
            ["sign in options"]    = "ms-settings:signinoptions",
            ["sign in"]            = "ms-settings:signinoptions",
            ["family"]             = "ms-settings:family-group",
            ["sync"]               = "ms-settings:sync",
            ["time"]               = "ms-settings:dateandtime",
            ["date"]               = "ms-settings:dateandtime",
            ["date and time"]      = "ms-settings:dateandtime",
            ["region"]             = "ms-settings:regionformatting",
            ["language"]           = "ms-settings:regionlanguage",
            ["accessibility"]      = "ms-settings:easeofaccess",
            ["ease of access"]     = "ms-settings:easeofaccess",
            ["narrator"]           = "ms-settings:easeofaccess-narrator",
            ["magnifier"]          = "ms-settings:easeofaccess-magnifier",
            ["contrast"]           = "ms-settings:easeofaccess-highcontrast",
            ["privacy"]            = "ms-settings:privacy",
            ["camera privacy"]     = "ms-settings:privacy-webcam",
            ["microphone privacy"] = "ms-settings:privacy-microphone",
            ["location"]           = "ms-settings:privacy-location",
            ["windows update"]     = "ms-settings:windowsupdate",
            ["update"]             = "ms-settings:windowsupdate",
            ["security"]           = "ms-settings:windowsdefender",
            ["defender"]           = "ms-settings:windowsdefender",
            ["firewall"]           = "ms-settings:windowsdefender",
            ["recovery"]           = "ms-settings:recovery",
            ["backup"]             = "ms-settings:backup",
            ["activation"]         = "ms-settings:activation",
            ["developer mode"]     = "ms-settings:developers",
            ["developers"]         = "ms-settings:developers",
            ["apps"]               = "ms-settings:appsfeatures",
            ["default apps"]       = "ms-settings:defaultapps",
            ["startup apps"]       = "ms-settings:startupapps",
            ["startup"]            = "ms-settings:startupapps",
            ["optional features"]  = "ms-settings:optionalfeatures",
            ["gaming"]             = "ms-settings:gaming-gamebar",
            ["game bar"]           = "ms-settings:gaming-gamebar",
            ["game mode"]          = "ms-settings:gaming-gamemode",
            ["clipboard"]          = "ms-settings:clipboard",
        };

        // ── App name → executable / URI map ──────────────────────────────────
        private static readonly Dictionary<string, string> AppMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Browsers
            ["chrome"]             = "chrome",
            ["google chrome"]      = "chrome",
            ["firefox"]            = "firefox",
            ["edge"]               = "msedge",
            ["microsoft edge"]     = "msedge",
            ["brave"]              = "brave",
            ["opera"]              = "opera",
            // Dev
            ["vs code"]            = "code",
            ["vscode"]             = "code",
            ["visual studio code"] = "code",
            ["visual studio"]      = "devenv",
            ["notepad"]            = "notepad",
            ["notepad++"]          = "notepad++",
            ["powershell"]         = "powershell",
            ["terminal"]           = "wt",
            ["windows terminal"]   = "wt",
            ["cmd"]                = "cmd",
            ["command prompt"]     = "cmd",
            ["cursor"]             = "cursor",
            ["android studio"]     = "studio64",
            ["unity"]              = "unityhub",
            ["unity hub"]          = "unityhub",
            ["postman"]            = "postman",
            ["figma"]              = "figma",
            ["docker"]             = "docker desktop",
            ["docker desktop"]     = "docker desktop",
            // Creative / Media
            ["paint"]              = "mspaint",
            ["ms paint"]           = "mspaint",
            ["photos"]             = "ms-photos:",
            ["camera"]             = "microsoft.windows.camera:",
            ["snipping tool"]      = "snippingtool",
            ["spotify"]            = "spotify",
            ["vlc"]                = "vlc",
            ["obs"]                = "obs64",
            ["obs studio"]         = "obs64",
            // Productivity
            ["word"]               = "winword",
            ["excel"]              = "excel",
            ["powerpoint"]         = "powerpnt",
            ["outlook"]            = "outlook",
            ["onenote"]            = "onenote",
            ["teams"]              = "msteams",
            ["microsoft teams"]    = "msteams",
            ["slack"]              = "slack",
            ["zoom"]               = "zoom",
            ["notion"]             = "notion",
            ["discord"]            = "discord",
            ["whatsapp"]           = "whatsapp:",
            ["telegram"]           = "telegram",
            // System utilities
            ["calculator"]         = "calc",
            ["settings"]           = "ms-settings:",
            ["control panel"]      = "control",
            ["task manager"]       = "taskmgr",
            ["file explorer"]      = "explorer",
            ["explorer"]           = "explorer",
            ["registry editor"]    = "regedit",
            ["registry"]           = "regedit",
            ["device manager"]     = "devmgmt.msc",
            ["disk management"]    = "diskmgmt.msc",
            ["event viewer"]       = "eventvwr.msc",
            ["services"]           = "services.msc",
            ["resource monitor"]   = "resmon",
            ["system info"]        = "msinfo32",
            ["system information"] = "msinfo32",
            ["store"]              = "ms-windows-store:",
            ["microsoft store"]    = "ms-windows-store:",
            // Games / fun
            ["steam"]              = "steam",
            ["epic games"]         = "epicgameslauncher",
        };

        public static string Execute(string action, string? param1 = null, string? param2 = null)
        {
            try
            {
                string act = (action ?? "").ToLower().Trim();
                string p1  = (param1  ?? "").Trim();
                string p2  = (param2  ?? "").Trim();

                // 1. Settings page
                if (act.Contains("setting"))
                    return OpenSettings(p1);

                // 2. Create folder
                if (act.Contains("create") && (act.Contains("folder") || act.Contains("dir")))
                    return CreateFolder(p1);

                // 3. Open folder
                if (act is "open_folder" || (act.Contains("folder") && !act.Contains("create")))
                    return OpenFolder(p1);

                // 4. Web search
                if (act is "search_web" || act.Contains("search"))
                    return WebSearch(p1);

                // 5. Open URL
                if (act is "open_url" || act.Contains("url") || act.Contains("website") || act.Contains("browser"))
                    return OpenUrl(p1);

                // 6. Open app / program
                if (act is "open_app" || act.Contains("app") || act.Contains("launch") || act.Contains("program") || act.Contains("open"))
                    return OpenApp(p1, p2);

                // 7. System commands
                if (act.Contains("shutdown"))    return RunShell("shutdown /s /t 10", "Shutting down in 10 seconds.");
                if (act.Contains("restart"))     return RunShell("shutdown /r /t 10", "Restarting in 10 seconds.");
                if (act.Contains("sleep"))       return RunShell("rundll32.exe powrprof.dll,SetSuspendState 0,1,0", "Going to sleep.");
                if (act.Contains("lock"))        return RunShell("rundll32.exe user32.dll,LockWorkStation", "Locked workstation.");
                if (act.Contains("screenshot"))  return TakeScreenshot();
                if (act.Contains("volume") || act.Contains("mute"))  return VolumeCmd(p1.Length > 0 ? p1 : act);
                if (act.Contains("clipboard"))   return OpenApp("ms-settings:clipboard", "");
                if (act.Contains("task manager"))return OpenApp("taskmgr", "");

                // 8. Fallback: maybe param1 is a URL
                if (!string.IsNullOrEmpty(p1) && (p1.StartsWith("http") || p1.Contains(".com") || p1.Contains(".org") || p1.Contains(".io") || p1.Contains("www.")))
                    return OpenUrl(p1);

                return $"I'm not sure how to handle '{action}' yet.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        public static string OpenSettings(string page)
        {
            if (string.IsNullOrWhiteSpace(page)) { Launch("ms-settings:"); return "Opened Settings."; }

            // Exact match
            if (SettingsMap.TryGetValue(page, out var uri)) { Launch(uri); return $"Opened {page} settings."; }

            // Fuzzy match
            string pageLow = page.ToLower();
            foreach (var kv in SettingsMap)
            {
                if (kv.Key.ToLower().Contains(pageLow) || pageLow.Contains(kv.Key.ToLower()))
                {
                    Launch(kv.Value);
                    return $"Opened {kv.Key} settings.";
                }
            }

            // Fallback: open main Settings
            Launch("ms-settings:");
            return $"Opened Settings (couldn't find '{page}' specifically).";
        }

        public static string OpenApp(string appName, string args)
        {
            if (string.IsNullOrWhiteSpace(appName)) return "Error: No app name provided.";

            string resolved = appName;
            if (AppMap.TryGetValue(appName, out var mapped))
            {
                resolved = mapped;
            }
            else
            {
                // Fuzzy match
                string low = appName.ToLower();
                var match = AppMap.FirstOrDefault(kv =>
                    low.Contains(kv.Key.ToLower()) || kv.Key.ToLower().Contains(low));
                if (!string.IsNullOrEmpty(match.Value)) resolved = match.Value;
            }

            // URI protocol (ms-settings:, ms-photos:, spotify:, etc.)
            if (resolved.Contains(':') && !Path.IsPathRooted(resolved))
            {
                Launch(resolved);
                return $"Opened {appName}.";
            }

            Process.Start(new ProcessStartInfo { FileName = resolved, Arguments = args, UseShellExecute = true });
            return $"Opened {appName}.";
        }

        private static string WebSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return "Error: No query provided.";
            Launch($"https://www.google.com/search?q={Uri.EscapeDataString(query)}");
            return $"Searched for '{query}'.";
        }

        private static string OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "Error: No URL provided.";
            if (!url.StartsWith("http")) url = "https://" + url;
            Launch(url);
            return $"Opened {url}.";
        }

        private static string CreateFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "Error: No folder name provided.";
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), path);
            Directory.CreateDirectory(path);
            Launch(path);
            return $"Created and opened folder: {path}";
        }

        private static string OpenFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { Launch("explorer"); return "Opened File Explorer."; }
            if (!Path.IsPathRooted(path))
            {
                string? known = path.ToLower() switch
                {
                    "desktop"   => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "downloads" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    "pictures"  => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "music"     => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    "videos"    => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    "home"      => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    _           => null
                };
                if (known != null) path = known;
            }
            Launch(path);
            return $"Opened {path}.";
        }

        private static string TakeScreenshot()
        {
            Process.Start(new ProcessStartInfo { FileName = "snippingtool", UseShellExecute = true });
            return "Opening Snipping Tool.";
        }

        private static string VolumeCmd(string direction)
        {
            try
            {
                string arg = direction.ToLower() switch
                {
                    "mute"      => "mutesysvolume 1",
                    var s when s.Contains("up") || s.Contains("increase")   => "changesysvolume 6554",
                    var s when s.Contains("down") || s.Contains("decrease") => "changesysvolume -6554",
                    _           => "mutesysvolume 2" // toggle
                };
                Process.Start(new ProcessStartInfo { FileName = "nircmd", Arguments = arg, UseShellExecute = false, CreateNoWindow = true });
                return $"Volume: {direction}.";
            }
            catch
            {
                Launch("ms-settings:sound");
                return "Opened Sound settings.";
            }
        }

        private static string RunShell(string cmd, string msg)
        {
            Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = $"/c {cmd}", UseShellExecute = false, CreateNoWindow = true });
            return msg;
        }

        private static void Launch(string target)
        {
            Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
        }
    }
}

