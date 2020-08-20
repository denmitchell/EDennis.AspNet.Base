using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Base class for launching multiple web applications.
    /// 
    /// Launcher is helpful for launching multiple projects at the same time, 
    /// while selecting specific launch profiles for each project -- either
    /// interactively or from an xUnit test.
    /// 
    /// Launcher requires commandline arguments that specify which launch profile is
    /// to be used for each project.  The launch profile argument is keyed by the
    /// project name, and the value is the name of the launch profile.
    /// 
    /// To use Launcher, you need to create a separate Console project for the launcher,
    /// create project references to each project that you want to launch, and have the
    /// console project's Program class extend LauncherBase.  To use Launcher
    /// interactively, follow the example in EDennis.Samples.ColorApp.Launcher -- 
    /// paying attention to launchSettings.json and the Program class.  To use
    /// Launcher from an xUnit test, call the Launch method directly from either
    /// your unit test or a Fixture and ensure that you pass in ewhAllSuspend={SOME_GUID_VALUE}
    /// as a commandline argument.  This value is used to unblock the "run" threads,
    /// allowing the applications to stop.
    /// 
    /// SPECIAL NOTE: Launcher propagates configuration settings by packing them
    ///   up into command-line arguments.  Configuration settings that have embedded
    ///   spaces or = in the values (e.g., connection strings) are quoted.  You 
    ///   have to remove the quotes in the target application.  EDennis.NetStandard.Base
    ///   has an IConfiguration extension method called GetValueOrThrow, which
    ///   by default removes quotes around configuration values.
    /// SPECIAL NOTE: Launcher does not work for unhosted client-side Blazor apps.  You
    ///   have to launch those apps separately.
    /// </summary>
    public abstract class LauncherBase : ILauncher {

        /// <summary>
        /// Implement this method by calling the Launch overload that accepts a
        /// param array of Action<string[]>[] -- a list of Program.Main methods
        /// to invoke.</string>
        /// </summary>
        /// <param name="args">Arguments to pass into the Launch method.  Note: for non-interactive
        /// sessions (e.g., xunit tests), you must pass in ewhAllSuspend={SOME_GUID_VALUE} without
        /// the braces.  This argument is used by the caller to suspend all web apps when they
        /// are no longer needed.</param>
        /// <param name="logger">Any ILogger instance or NullLogger.Instance for automated testing</param>
        /// <param name="launchBrowser">true if the browser should be launched for any web app whose
        /// targeted launch profile has a launchUrl</param>
        /// <param name="blockWithConsole">set to true if this is an interactive (non-xunit) session</param>
        public abstract void Launch(string[] args, bool launchBrowser = false, bool blockWithConsole = false);

        /// <summary>
        /// How long to try to connect to any given web app.
        /// </summary>
        public const int PING_TIMEOUT = 10000;

        private ILogger _logger;

        public LauncherBase(ILogger logger) {
            _logger = logger;
        }


        /// <summary>
        /// Launch a set of web applications
        /// </summary>
        /// <param name="args">Arguments to pass into the Launch method.  Note: for non-interactive
        /// sessions (e.g., xunit tests), you must pass in ewhAllSuspend={SOME_GUID_VALUE} without
        /// the braces.  This argument is used by the caller to suspend all web apps when they
        /// are no longer needed.</param>
        /// <param name="logger">Any ILogger instance or NullLogger.Instance for automated testing</param>
        /// <param name="launchBrowser">true if the browser should be launched for any web app whose
        /// targeted launch profile has a launchUrl</param>
        /// <param name="blockWithConsole">set to true if this is an interactive (non-xunit) session</param>
        /// <param name="programMains">A parameter array of Program.Main methods to invoke</param>
        /// <returns></returns>
        public Dictionary<string, Launchable> Launch(string[] args, bool blockWithConsole,
                params Action<string[]>[] programMains) {

            //get the project name, directory, and .csproj file path associated with this Launcher class
            var projectName = GetType().Assembly.GetName().Name;
            var launcherDirectory = GetProjectDirectory(projectName);
            var csprojPath = $"{launcherDirectory}\\{projectName}.csproj";

            //get the project directories for all web applications to launch.
            var dirs = GetProjectDirectories(csprojPath,launcherDirectory);

            //build a Launchable object for each web application to launch
            //and store in a dictionary, keyed by the project name
            var launchables = InitializeLaunchables(args, programMains, dirs);

            CreateConsoleLogger();

            //iterate over all the launchables, Launching each one
            foreach (var launchable in launchables) {
                Launch(launchable, blockWithConsole);

                //NOTE: the following does not seem to be needed
                //Wait for the current server to be ready before moving on to the next server.
                //launchable.Value.ReadyEvent.WaitOne();
            }
            return launchables;
        }

        /// <summary>
        /// Launch Browser tabs for each web app whose active launch profile indicates
        /// that the browser should be launched.  Note: launching browsers is controlled
        /// globally by the launchBrowser parameter of the Launch method
        /// </summary>
        /// <param name="launchables"></param>
        public void LaunchBrowsers(Dictionary<string, Launchable> launchables) {
            foreach (var launchable in launchables) {
                var profile = launchable.Value.LaunchProfile;
                if (profile.LaunchBrowser) {
                    var httpsUrl = profile.ApplicationUrls.Single(u => u.Scheme == "https");

                    OpenBrowser($"https://{httpsUrl.Host}:{httpsUrl.Port}/{profile.LaunchUrl}");
                }
            }
        }

        /// <summary>
        /// Launches an individual web app
        /// </summary>
        /// <param name="launchable">launch-relevant information packaged in an object</param>
        /// <param name="blockWithConsole">whether to block the threads from completing interactively 
        /// (or via NamedEventWaitHandle)</param>
        /// <param name="logger">An ILogger to use for logging launch progress</param>
        private void Launch(KeyValuePair<string, Launchable> launchable, bool blockWithConsole) {

            GetLaunchProfile(launchable);
            GetCommandLineArgs(launchable);

            Task.Run(() => {
                launchable.Value.ProgramMain(launchable.Value.LaunchProfile.Args);
                Ping(launchable);
                if (!blockWithConsole)
                    launchable.Value.AllSuspendEvent.WaitOne();
            });

        }

        /// <summary>
        /// Checks to see if a given web app can be reached via TCP.
        /// </summary>
        /// <param name="launchable"></param>
        private void Ping(KeyValuePair<string, Launchable> launchable) {

            var httpsUrl = launchable.Value.LaunchProfile.ApplicationUrls.Single(u => u.Scheme == "https");

            Task.Run(() => {

                var sw = new Stopwatch();

                sw.Start();
                while (sw.ElapsedMilliseconds < PING_TIMEOUT) {
                    try {
                        using var tcp = new TcpClient(httpsUrl.Host, httpsUrl.Port);
                        var connected = tcp.Connected;
                        //launchable.Value.ReadyEvent.Set();
                        _logger.LogInformation($"Successfully pinged {launchable.Key} @ https://{httpsUrl.Host}:{httpsUrl.Port}");
                        return;
                    } catch (Exception ex) {
                        if (!ex.Message.Contains("No connection could be made because the target machine actively refused it")) {
                            _logger.LogError(ex, ex.Message);
                            throw ex;
                        } else
                            Thread.Sleep(1000);
                    }

                }

                var ex1 = new Exception($"Cannot ping {launchable.Key} @ https://{httpsUrl.Host}:{httpsUrl.Port}");
                _logger.LogError(ex1.Message);
                throw ex1;

            });

        }

        /// <summary>
        /// Builds a dictionary of Launchable objects, which contains all of the relevant
        /// information for launching the web applications
        /// </summary>
        /// <param name="args">global arguments</param>
        /// <param name="programMains">an array of Program.Main methods to invoke</param>
        /// <param name="dirs">a dictionary of project locations, keyed by the project names</param>
        /// <returns></returns>
        private static Dictionary<string, Launchable> InitializeLaunchables(string[] args,
            Action<string[]>[] programMains, Dictionary<string, string> dirs) {

            var launchables = new Dictionary<string, Launchable>();

            var kvpArgs = args.Select(a => new KeyValuePair<string,string>(a.Split('=')[0], a.Split('=')[1]))
                .ToDictionary(x => x.Key, x => x.Value);

            NamedEventWaitHandle ewhReady = null;
            NamedEventWaitHandle ewhAllSuspend = null;

            //used with pinging -- may not be needed
            ewhReady = new NamedEventWaitHandle(false, EventResetMode.ManualReset);

            //used to suspend all web application from the caller (typically xunit runner/test host)
            if (kvpArgs.TryGetValue("ewhAllSuspend", out string ewhAllSuspendName))
                ewhAllSuspend = new NamedEventWaitHandle(false, EventResetMode.ManualReset, ewhAllSuspendName);

            //loop over all Program.Main methods, building a launchable for each method
            foreach (var programMain in programMains) {
                var projectName = programMain.Method.DeclaringType.Assembly.GetName().Name;
                var launchable = new Launchable {
                    ProgramMain = programMain,
                    LaunchProfile = new LaunchProfile(),
                    ReadyEvent = ewhReady,
                    AllSuspendEvent = ewhAllSuspend
                };

                //the full launch profile will be retrieved and populated later.
                //For now, just store the requested profile name.
                if (kvpArgs.TryGetValue(projectName, out string requestedProfile))
                    launchable.LaunchProfile.Name = requestedProfile;
                else
                    throw new ArgumentException($"Launch profile name not supplied for {projectName}.  Launcher requires a command-line argument like {projectName}=MyLaunchProfile with a valid launch profile name.  This also applies to other launched projects.  They must have a launch profile as a commandline argument, keyed by the project name.");

                //set the project directory.
                if (dirs.TryGetValue(projectName, out string dir))
                    launchable.ProjectDirectory = dir;
                else
                    throw new ArgumentException($"{projectName} not a referenced project in Launcher's .csproj");

                launchables.Add(projectName, launchable);
            }
            return launchables;
        }


        /// <summary>
        /// Gets a dictionary of all project directories referenced by the Launcher, using
        /// entries in the Launcher's .csproj file
        /// </summary>
        /// <param name="csprojPath"></param>
        /// <param name="baseDir"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetProjectDirectories(string csprojPath, string baseDir) {

            XmlDocument doc = new XmlDocument();
            doc.Load(csprojPath);
            XmlNode root = doc.DocumentElement;

            XmlNodeList nodeList = root.SelectNodes("descendant::ItemGroup/ProjectReference");

            var dirs = new Dictionary<string, string>();
            foreach (XmlNode proj in nodeList) {
                var filePath = ((XmlElement)proj).GetAttributeNode("Include").Value;
                var dirPath = filePath.Substring(0, filePath.LastIndexOf('\\'));
                var projName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                projName = projName.Substring(0, projName.LastIndexOf('.'));
                var path = Path.GetFullPath(Path.Combine(baseDir, dirPath));
                dirs.Add(projName, path);
            }

            return dirs;
        }



        /// <summary>
        /// Gets a project directory for a project.  Note that this requires a
        /// workaround when executing from an xunit test because the
        /// Launcher's assembly resides in the test project's directory
        /// structure.
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        private static string GetProjectDirectory(string projectName) {
            var dir = AppContext.BaseDirectory;
            string[] files = Directory.GetFiles(dir, "*.csproj");
            var hasCsProj = File.Exists($"{dir}/{projectName}.csproj");
            while (files.Length == 0) {
                dir = Directory.GetParent(dir).FullName;
                files = Directory.GetFiles(dir, "*.csproj");
                if (File.Exists($"{dir}/{projectName}.csproj"))
                    return dir; //launcher (non-xunit) entrypoint
            }
            //handle xunit entrypoint
            var dirs = GetProjectDirectories(files[0], dir);
            return dirs.Where(x => x.Key == projectName).Select(x => x.Value).FirstOrDefault();
        }

        /// <summary>
        /// Populate a launchable's CommandLineArgs with all IConfiguration entries that
        /// reside in a relevant appsettings file.  Injecting the configuration entries
        /// this way allows us to otherwise blackbox the configuration work of
        /// the individual web apps 
        /// </summary>
        /// <param name="launchable"></param>
        private static void GetCommandLineArgs(KeyValuePair<string, Launchable> launchable) {
            var launchProfile = launchable.Value.LaunchProfile;

            //get the environment name.  Override the OS environment setting if there
            //is a setting in the target launch profile.
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (launchProfile.EnvironmentVariables.TryGetValue("ASPNETCORE_ENVIRONMENT", out string env2))
                env = env2;

            var dir = launchable.Value.ProjectDirectory;

            //build configuration from possible appsettings file locations
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile($"{dir}/appsettings.json", true);
            builder.AddJsonFile($"{dir}/appsettings.{env}.json", true);
            builder.AddJsonFile($"{dir}/wwwroot/appsettings.json", true);
            builder.AddJsonFile($"{dir}/wwwroot/appsettings.{env}.json", true);
            var config = builder.Build();

            //flatten the configurations into simple key-value pairs
            List<string> args = new List<string>();
            var flattened = Flatten(config);
            foreach (var entry in flattened) {
                if (entry.Value.Contains(' ') || entry.Value.Contains('='))
                    args.Add($"{entry.Key}=\"{entry.Value}\"");
                else
                    args.Add($"{entry.Key}={entry.Value}");
            }

            //add settings for the URLs from the target launch profile
            args.Add($"URLS={launchProfile.ApplicationUrl}");
            args.Add($"HTTPS_PORT={launchProfile.ApplicationUrls.Single(u => u.Scheme == "https").Port}");

            //add/overwrite commandLineArgs with those specified in launch profile
            if (launchable.Value.LaunchProfile.CommandLineArgs != null) {
                var lpArgs = launchable.Value.LaunchProfile.CommandLineArgs.Split(' ');
                foreach (var lpArg in lpArgs) {
                    var components = lpArg.Split('=');
                    if (components.Count() == 2) {
                        for (int i = 0; i < args.Count(); i++) {
                            if (args[i].StartsWith(components[0] + "=")) {
                                args.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    args.Add(lpArg);
                }
            }

            //return as an array of commandline arguments
            launchable.Value.LaunchProfile.Args = args.ToArray();
        }

        /// <summary>
        /// Implements flattening of configurations
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Dictionary<string, string> Flatten(IConfiguration config) {
            var dict = new Dictionary<string, string>();
            config.GetChildren().AsParallel().ToList()
                .ForEach(x => Flatten(x, dict));
            return dict;
        }

        /// <summary>
        /// Recursive flattening of configurations
        /// </summary>
        /// <param name="section"></param>
        /// <param name="dict"></param>
        /// <param name="parentKey"></param>
        private static void Flatten(IConfigurationSection section,
            Dictionary<string, string> dict, string parentKey = "") {
            if (section.Value == null)
                section.GetChildren().AsParallel().ToList()
                    .ForEach(x => Flatten(x, dict, $"{parentKey}{section.Key}:"));
            else
                dict.Add($"{parentKey}{section.Key}", $"{section.Value}");
        }


        /// <summary>
        /// Builds the LaunchProfile object for a specific web application by 
        /// retrieving and parsing the relevant launchSettings.json file.
        /// </summary>
        /// <param name="launchable"></param>
        private static void GetLaunchProfile(KeyValuePair<string, Launchable> launchable) {

            var filePath = $"{launchable.Value.ProjectDirectory}\\Properties\\launchSettings.json";
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            var doc = JsonDocument.Parse(json);
            var el = doc.RootElement;
            var profiles = el.GetProperty("profiles");

            var launchProfile = new LaunchProfile();
            JsonProperty filtered = default;

            if (launchable.Value.LaunchProfile?.Name != null)
                filtered = profiles.EnumerateObject().FirstOrDefault(x => x.Name == launchable.Value.LaunchProfile.Name);
            else
                filtered = profiles.EnumerateObject().FirstOrDefault(x => x.Value.EnumerateObject().Any(p => p.Name == "commandName" && p.Value.GetString() == "Project"));

            launchProfile.Name = filtered.Name;

            var profile = filtered.Value;
            foreach (var prop in profile.EnumerateObject()) {
                if (prop.Name == "commandName")
                    launchProfile.CommandName = prop.Value.GetString();
                else if (prop.Name == "applicationUrl") {
                    launchProfile.ApplicationUrls = prop.Value.GetString().Split(';').Select(x => new ApplicationUrl(x)).ToArray();
                } else if (prop.Name == "launchBrowser")
                    launchProfile.LaunchBrowser = prop.Value.GetBoolean();
                else if (prop.Name == "launchUrl")
                    launchProfile.LaunchUrl = prop.Value.GetString();
                else if (prop.Name == "commandLineArgs")
                    launchProfile.Args = prop.Value.GetString().Split(' ');
                else if (prop.Name == "environmentVariables") {
                    foreach (var ev in prop.Value.EnumerateObject()) {
                        launchProfile.EnvironmentVariables.Add(ev.Name, ev.Value.GetString());
                    }
                }
            }
            launchable.Value.LaunchProfile = launchProfile;
        }

        /// <summary>
        /// Opens a browser -- for interactive sessions
        /// </summary>
        /// <param name="url"></param>
        private static void OpenBrowser(string url) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process.Start("xdg-open", url);  // Works ok on linux
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start("open", url); // Not tested
            } else {
                throw new Exception("Cannot open browser");
            }
        }


        private void CreateConsoleLogger() {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());

            var serviceProvider = services.BuildServiceProvider();
            var _logger = serviceProvider.GetService<ILogger<LauncherBase>>();
        }

    }
}
