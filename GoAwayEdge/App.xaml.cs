﻿/*
 * Go away Edge - IFEO Method
 * by valnoxy (valnoxy.dev)
 * ----------------------------------
 *  HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\msedge.exe
 *   > UseFilter (DWORD) = 1
 *
 *  HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\msedge.exe\0
 *   > Debugger (REG_SZ) = "Path\To\GoAwayEdge.exe"
 *   > FullFilterPath (REG_SZ) = C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe
 */

using GoAwayEdge.Common;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Windows;

namespace GoAwayEdge
{
    /// <summary>
    /// Interaktionslogik für App.xaml
    /// </summary>
    public partial class App
    {
        private static string? _url;
        public static bool Debug = false;

        public void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            Debug = true;
#endif
            // Load Language
            LocalizationManager.LoadLanguage();

            string[] args = e.Args;
            if (args.Length == 0) // Opens Installer
            {
                if (IsAdministrator() == false)
                {
                    // Restart program and run as admin
                    var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                    if (exeName != null)
                    {
                        var startInfo = new ProcessStartInfo(exeName)
                        {
                            Verb = "runas",
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                    }
                    Environment.Exit(0);
                    return;
                }

                var installer = new Installer();
                installer.ShowDialog();
                Environment.Exit(0);
            }
            else if (args.Length > 0)
            {
                if (args.Contains("-ToastActivated")) // Clicked on notification, ignore it.
                    Environment.Exit(0);
                if (args.Contains("-debug"))
                    Debug = true;
                if (args.Contains("-s")) // Silent Installation
                {
                    foreach (var arg in args)
                    {
                        if (arg.StartsWith("-se:"))
                            Configuration.Search = ParseSearchEngine(arg);
                        if (arg.Contains("--url:"))
                        {
                            Configuration.CustomQueryUrl = ParseCustomSearchEngine(arg);
                            Configuration.Search = !string.IsNullOrEmpty(Configuration.CustomQueryUrl) ? SearchEngine.Custom : SearchEngine.Google;
                        }
                    }

                    if (IsAdministrator() == false)
                    {
                        // Restart program and run as admin
                        var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                        if (exeName != null)
                        {
                            var startInfo = new ProcessStartInfo(exeName)
                            {
                                Verb = "runas",
                                UseShellExecute = true,
                                Arguments = string.Join(" ", args)
                            };
                            Process.Start(startInfo);
                        }
                        Environment.Exit(0);
                        return;
                    }

                    Configuration.InitialEnvironment();
                    InstallRoutine.Install(null);
                    Environment.Exit(0);
                }

                if (args.Contains("-u"))
                {
                    InstallRoutine.Uninstall(null);
                    Environment.Exit(0);
                }
                if (args.Contains("--update"))
                {
                    var statusEnv = Configuration.InitialEnvironment();
                    if (statusEnv == false) Environment.Exit(1);

                    // Check for app update
                    var updateAvailable = Updater.CheckForAppUpdate();
                    if (!string.IsNullOrEmpty(updateAvailable))
                    {
                        var updateMessage = LocalizationManager.LocalizeValue("NewUpdateAvailable", updateAvailable);
                        var remindMeLaterBtn = LocalizationManager.LocalizeValue("RemindMeLater");
                        var installUpdateBtn = LocalizationManager.LocalizeValue("InstallUpdate");

                        var updateDialog = new MessageUi("GoAwayEdge", updateMessage, installUpdateBtn, remindMeLaterBtn, true);
                        updateDialog.ShowDialog();
                        if (updateDialog.Summary == "Btn1")
                        {
                            var updateResult = Updater.UpdateClient();
                            if (!updateResult) Environment.Exit(0);
                        }
                    }

                    // Validate Ifeo binary
                    var binaryStatus = Updater.ValidateIfeoBinary();
                    switch (binaryStatus)
                    {
                        case 0: // validated
                            break;
                        case 1: // failed validation
                            if (IsAdministrator() == false)
                            {
                                var updateNonIfeoMessage = LocalizationManager.LocalizeValue("NewNonIfeoUpdate");
                                var remindMeLaterBtn = LocalizationManager.LocalizeValue("RemindMeLater");
                                var installUpdateBtn = LocalizationManager.LocalizeValue("InstallUpdate");

                                var ifeoMessageUi = new MessageUi("GoAwayEdge", updateNonIfeoMessage, installUpdateBtn, remindMeLaterBtn);
                                ifeoMessageUi.ShowDialog();

                                if (ifeoMessageUi.Summary == "Btn1")
                                {
                                    // Restart program and run as admin
                                    var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                                    if (exeName != null)
                                    {
                                        var startInfo = new ProcessStartInfo(exeName)
                                        {
                                            Verb = "runas",
                                            UseShellExecute = true,
                                            Arguments = "--update"
                                        };
                                        Process.Start(startInfo);
                                    }
                                    Environment.Exit(0);
                                    return;
                                }
                                Environment.Exit(0);
                            }
                            Updater.ModifyIfeoBinary(ModifyAction.Update);
                            break;
                        case 2: // missing
                            if (IsAdministrator() == false)
                            {
                                var ifeoMissingMessage = LocalizationManager.LocalizeValue("MissingIfeoFile");
                                var yesBtn = LocalizationManager.LocalizeValue("Yes");
                                var noBtn = LocalizationManager.LocalizeValue("No");
                                var ifeoMessageUi = new MessageUi("GoAwayEdge", ifeoMissingMessage, yesBtn, noBtn);
                                ifeoMessageUi.ShowDialog();

                                if (ifeoMessageUi.Summary == "Btn1")
                                {
                                    // Restart program and run as admin
                                    var exeName = Process.GetCurrentProcess().MainModule?.FileName;
                                    if (exeName != null)
                                    {
                                        var startInfo = new ProcessStartInfo(exeName)
                                        {
                                            Verb = "runas",
                                            UseShellExecute = true,
                                            Arguments = "--update"
                                        };
                                        Process.Start(startInfo);
                                    }
                                    Environment.Exit(0);
                                    return;
                                }
                                Environment.Exit(0);
                            }
                            Updater.ModifyIfeoBinary(ModifyAction.Create);
                            break;
                    }
                    Environment.Exit(0);
                }
            }

            Configuration.InitialEnvironment();
            try
            {
                Configuration.Search = ParseSearchEngine(RegistryConfig.GetKey("SearchEngine"));
            }
            catch
            {
                // ignored
            }
            RunParser(args);

            Environment.Exit(0);
        }

        public static void RunParser(string[] args)
        {
            var argumentJoin = string.Join(" ", args);
            var isFile = false;
            var isCopilot = false;
            var parsedData = false;
            var ignoreStartup = false;
            var p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.RedirectStandardOutput = false;

            if (Debug)
            {
                var w = new MessageUi("GoAwayEdge",
                    $"The following args are redirected (CTRL+C to copy):\n\n{argumentJoin}", "OK", null, true);
                w.ShowDialog();
            }

            foreach (var arg in args)
            {
                // Check for Copilot
                if (arg.Contains("microsoft-edge://?ux=copilot&tcp=1&source=taskbar")
                    || arg.Contains("microsoft-edge:///?ux=copilot&tcp=1&source=taskbar"))
                {
                    _url = arg;
                    isCopilot = true;
                    break;
                }

                // User want to parse data
                if (arg.Contains("microsoft-edge:"))
                {
                    _url = ParseUrl(arg);
                    parsedData = true;
                    break;
                }

                // Check if the argument contains a file (like PDF)
                if (arg.Contains("msedge.exe"))
                    isFile = false;
                else if (File.Exists(arg))
                    isFile = true;

                // Check for blacklisted arguments
                if (arg.Contains("--no-startup-window") 
                    || arg.Contains("--profile-directory"))
                    ignoreStartup = true;
            }

            // Open Edge normally
            if ((!parsedData || isCopilot || isFile || args.Contains("--profile-directory")) && !ignoreStartup)
            {
                if (Debug)
                {
                    if (isCopilot)
                    {
                        var copilotMessageUi = new MessageUi("GoAwayEdge",
                            $"Opening Windows Copilot with following url:\n{_url}", "OK", null, true);
                        copilotMessageUi.ShowDialog();
                    }
                    else
                    {
                        var messageUi = new MessageUi("GoAwayEdge",
                            "Microsoft Edge will now start normally via IFEO application.", "OK", null, true);
                        messageUi.ShowDialog();
                    }
                }
                var parsedArgs = args.Skip(2);
                p.StartInfo.FileName = FileConfiguration.NonIfeoPath;
                p.StartInfo.Arguments = string.Join(" ", parsedArgs);
                p.Start();
                Environment.Exit(0);
            }

            // Open default Browser with parsed data
            if (parsedData)
            {
                if (Debug)
                {
                    var defaultUrlMessageUi = new MessageUi("GoAwayEdge",
                        "Opening URL in default browser:\n\n" + _url + "\n", "OK", null, true);
                    defaultUrlMessageUi.ShowDialog();
                }
                p.StartInfo.FileName = _url;
                p.StartInfo.Arguments = "";
                p.Start();
                Environment.Exit(0);
            }

            if (ignoreStartup)
            {
                if (Debug)
                {
                    var defaultUrlMessageUi = new MessageUi("GoAwayEdge",
                        "Edge was called with a blacklisted argument. Edge won't be launched.", "OK", null, true);
                    defaultUrlMessageUi.ShowDialog();
                }
                Environment.Exit(0);
            }
        }

        private static string? ParseCustomSearchEngine(string argument)
        {
            var argParsed = argument.Remove(0, 6);
            var result = Uri.TryCreate(argParsed, UriKind.Absolute, out var uriResult)
                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result ? argParsed : null;
        }

        private static SearchEngine ParseSearchEngine(string argument)
        {
            var arg = argument;
            if (argument.StartsWith("-se:"))
                arg = argument.Remove(0, 4);
            
            return arg.ToLower() switch
            {
                "google" => SearchEngine.Google,
                "bing" => SearchEngine.Bing,
                "duckduckgo" => SearchEngine.DuckDuckGo,
                "yahoo" => SearchEngine.Yahoo,
                "yandex" => SearchEngine.Yandex,
                "ecosia" => SearchEngine.Ecosia,
                "ask" => SearchEngine.Ask,
                "qwant" => SearchEngine.Qwant,
                "perplexity" => SearchEngine.Perplexity,
                "custom" => SearchEngine.Custom,
                _ => SearchEngine.Google // Fallback search engine
            };
        }

        private static string ParseUrl(string encodedUrl)
        {
            // Remove URI handler with url argument prefix
            encodedUrl = encodedUrl[encodedUrl.IndexOf("http", StringComparison.Ordinal)..];

            // Remove junk after search term
            if (encodedUrl.Contains("https%3A%2F%2Fwww.bing.com%2Fsearch%3Fq%3D") && !encodedUrl.Contains("redirect"))
                encodedUrl = encodedUrl.Substring(encodedUrl.IndexOf("http", StringComparison.Ordinal), encodedUrl.IndexOf("%26", StringComparison.Ordinal));

            // Alternative url form
            if (encodedUrl.Contains("https%3A%2F%2Fwww.bing.com%2Fsearch%3Fform%3D"))
            {
                encodedUrl = encodedUrl.Substring(encodedUrl.IndexOf("26q%3D", StringComparison.Ordinal) + 6, encodedUrl.Length - (encodedUrl.IndexOf("26q%3D", StringComparison.Ordinal) + 6));
                encodedUrl = "https://www.bing.com/search?q=" + encodedUrl;
            }

            // Decode Url
            encodedUrl = encodedUrl.Contains("redirect") ? DotSlash(encodedUrl) : DecodeUrlString(encodedUrl);

            // Replace Search Engine
            encodedUrl = encodedUrl.Replace("https://www.bing.com/search?q=", DefineEngine(Configuration.Search));

            if (Debug)
            {
                var uriMessageUi = new MessageUi("GoAwayEdge",
                    "New Uri: " + encodedUrl, "OK", null, true);
                uriMessageUi.ShowDialog();
            }
            var uri = new Uri(encodedUrl);
            return uri.ToString();
        }

        private static string DefineEngine(SearchEngine engine)
        {
            var customQueryUrl = string.Empty;
            try
            {
                customQueryUrl = RegistryConfig.GetKey("CustomQueryUrl");
            }
            catch
            {
                // ignore; not an valid object
            }

            return engine switch
            {
                SearchEngine.Google => "https://www.google.com/search?q=",
                SearchEngine.Bing => "https://www.bing.com/search?q=",
                SearchEngine.DuckDuckGo => "https://duckduckgo.com/?q=",
                SearchEngine.Yahoo => "https://search.yahoo.com/search?p=",
                SearchEngine.Yandex => "https://yandex.com/search/?text=",
                SearchEngine.Ecosia => "https://www.ecosia.org/search?q=",
                SearchEngine.Ask => "https://www.ask.com/web?q=",
                SearchEngine.Qwant => "https://qwant.com/?q=",
                SearchEngine.Perplexity => "https://www.perplexity.ai/search?copilot=false&q=",
                SearchEngine.Custom => customQueryUrl,
                _ => "https://www.google.com/search?q="
            };
        }
        
        private static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        private static string DotSlash(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;

            try // Decode base64 string from url
            {
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query).Get("url");
                if (query != null)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(query));
                    return decoded;
                }
            }
            catch
            {
                // ignored
            }

            return url;
        }
        
        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
