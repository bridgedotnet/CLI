using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bridge.CLI
{
    public partial class Program
    {
        private static string[] DEFAULT_REFERENCES_PATHES = new string[] { "bin", "Libs" };

        public static Assembly TranslatorAssembly { get; private set; }

        public static Assembly ContractAssembly { get; private set; }

        public static string CoreFolder { get; private set; }

        private static int Main(string[] args)
        {
            //args = new string[] { "add", "repo", @"""C:\projects\Bridge\v1\Bridge\.build\packages\""", "-n", "local" };
            //var currentDir = @"C:\projects\Bridge\v1\Sandbox\FolderLib5\";
            var currentDir = Environment.CurrentDirectory;
            var msg = "";

            CoreFolder = GetCoreFolder(currentDir);
            var resolver = new AssemblyResolver(CoreFolder);
            AppDomain.CurrentDomain.AssemblyResolve += resolver.CurrentDomain_AssemblyResolve;

            TranslatorAssembly = GetTranslatorAssembly(CoreFolder);
            ContractAssembly = GetContractAssembly(CoreFolder);

            if (!EnsureMinimalCompilerVersion())
            {
                return 1;
            }

            if (args.Length == 0)
            {
                ShowHelp();

                return 1;
            }

            bool skip = false;
            bool run = false;
            dynamic bridgeOptions = null;

            /*try
            {
                bridgeOptions = GetBridgeOptionsFromCommandLine(currentDir, args, ref skip, ref run);
            }
            catch(Exception)
            {
                Error("Invalid command line");
                return 1;
            }*/
            bridgeOptions = GetBridgeOptionsFromCommandLine(currentDir, args, ref skip, ref run);

            if (bridgeOptions == null)
            {
                ShowHelp();

                return 1;
            }

            if (skip)
            {
                return 0;
            }

            if (bridgeOptions.Rebuild && File.Exists(bridgeOptions.Lib))
            {
                File.Delete(bridgeOptions.Lib);
            }

            if (bridgeOptions.BridgeLocation == null)
            {
                bridgeOptions.BridgeLocation = GetBridgeLocation(currentDir);
            }

            msg = $"Building {Path.GetFileName(currentDir)}";

            if (msg.Length >= 28)
            {
                msg += "  ";
            }
            else
            {
                msg = msg.PadRight(29);
            }

            Console.Write(msg);

            using (var spinner = new ConsoleSpinner())
            {
                spinner.Start();
                var logger = CreateLogger();
                dynamic processor = CreateTranslatorProcessor(bridgeOptions, logger);

                processor.PreProcess();

                try
                {
                    processor.Process();
                    var outputPath = processor.PostProcess();

                    if (run)
                    {
                        var htmlFile = Path.Combine(outputPath, "index.html");

                        if (File.Exists(htmlFile))
                        {
                            System.Diagnostics.Process.Start(htmlFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    spinner.Stop();

                    var exceptionType = ex.GetType();

                    if (GetEmitterException().IsAssignableFrom(exceptionType))
                    {
                        dynamic dex = ex;
                        Error(string.Format("Bridge.NET Compiler error: {2} ({3}, {4}) {0} {1}", ex.Message, ex.StackTrace, dex.FileName, dex.StartLine, dex.StartColumn));

                        return 1;
                    }

                    var ee = processor.Translator != null ? processor.Translator.CreateExceptionFromLastNode() : null;

                    if (ee != null)
                    {
                        Error(string.Format("Bridge.NET Compiler error: {2} ({3}, {4}) {0} {1}", ex.Message, ex.StackTrace, ee.FileName, ee.StartLine, ee.StartColumn));
                    }
                    else
                    {
                        // Iteractively print inner exceptions
                        var ine = ex;
                        var elvl = 0;

                        while (ine != null)
                        {
                            Error(string.Format("Bridge.NET Compiler error: exception level: {0} - {1}\nStack trace:\n{2}", elvl++, ine.Message, ine.StackTrace));
                            ine = ine.InnerException;
                        }
                    }

                    
                    return 1;
                }
            }

            Console.Write("done");

            return 0;
        }

        private static bool EnsureMinimalCompilerVersion()
        {
            var installedVersion = GetCompilerVersion().ProductVersion;
            var minVersion = "16.5.0";

            if (new Version(minVersion).CompareTo(new Version(installedVersion)) > 0)
            {
                Error($"Minimum required version of Bridge compiler is {minVersion}. Your version: {installedVersion}");

                return false;
            }

            return true;
        }

        private static System.Diagnostics.FileVersionInfo GetCompilerVersion()
        {
            return System.Diagnostics.FileVersionInfo.GetVersionInfo(TranslatorAssembly.Location);
        }

        private static void ShowVersion()
        {
            Console.WriteLine(GetCompilerVersion().ProductVersion);
        }

        /// <summary>
        /// Commandline arguments based on http://docopt.org/
        /// </summary>
        private static void ShowHelp()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string programName = Path.GetFileName(codeBase);

            Console.WriteLine(@"
Usage: 
  bridge [commands] [options]
  bridge [options] (<name>|<path>|<list>)
  bridge [-h|--help]


Commands:
  new                        Initialize a Bridge C# Class Library project.
  build                      Builds the Bridge project.
  run                        Compiles and immediately runs index.html file.
  restore                    Restore dependencies specified in the project.
  add package <name>         Add package to project
  remove package <name>      Remove package from project


Options:
  -c, --configuration <name> Configuration name (Debug/Release etc).
  -D, --define <list>        Semicolon-delimited list of project constants.
  -f, --folder <path>        Builder working directory relative to current dir.
  -h, --help                 Display help.
  -p, --project <path>       The .csproj file location.
  -P, --platform <name>      Platform name (AnyCPU etc).
  -r, --rebuild              Force assembly rebuilding.
  -R, --recursive            Recursively search for .cs source files.
  -s, --source <file>        Source files name/pattern [default: *.cs].
  -S, --settings <name:val>  Comma-delimited list of project settings.
  -v, --version              Display version.
  --nocore                   Do not extract core javascript files.
  --norecursive              Non-recursive search for .cs source files.
  --notimestamp              Do not show timestamp in log messages.


  More information on getting started with
  Bridge.NET at https://bridge.net/docs
");

#if DEBUG
            // This code and logic is only compiled in when building a Bridge project in Debug mode
            Console.WriteLine(@"  -d, --debug                    Attach the builder to a Visual Studio debugging
                               session. Use this to attach the process to an
                               open Bridge Solution. This option is equivalent
                               to Build.dll's 'AttachDebugger'.");
#endif
        }

        private static bool BindCmdArgumentToOption(string arg, dynamic bridgeOptions)
        {
            if (bridgeOptions.ProjectLocation == null && bridgeOptions.Lib == null)
            {
                if (arg.ToLower().EndsWith(".csproj"))
                {
                    bridgeOptions.ProjectLocation = arg;

                    return true;
                }
                else if (arg.ToLower().EndsWith(".dll"))
                {
                    bridgeOptions.Lib = arg;

                    return true;
                }
            }

            return false; // didn't bind anywhere
        }

        public static dynamic GetBridgeOptionsFromCommandLine(string currentDir, string[] args, ref bool skip, ref bool run)
        {
            dynamic bridgeOptions = CreateBridgeOptions();
            bridgeOptions.Recursive = true;

            bridgeOptions.Name = "";
            bridgeOptions.ProjectProperties = CreateProjectProperties();

            // options -c, -P and -D have priority over -S
            string configuration = null;
            var hasPriorityConfiguration = false;
            string platform = null;
            var hasPriorityPlatform = false;
            string defineConstants = null;
            var hasPriorityDefineConstants = false;

            int i = 0;

            while (i < args.Length)
            {
                switch (args[i])
                {
                    case "add":
                        string entity = null;

                        if (args.Length > (i + 1))
                        {
                            entity = args[++i];
                        }

                        switch (entity)
                        {
                            case "package":
                                string package = null;

                                if (args.Length > (i + 1))
                                {
                                    package = args[++i];
                                }

                                if (string.IsNullOrWhiteSpace(package))
                                {
                                    throw new Exception("Please include package name.");
                                }

                                string version = null;

                                if (args.Length > (i + 2) && (args[i + 1] == "-v" || args[i + 1] == "--version"))
                                {
                                    version = args[i + 2];
                                }

                                AddPackage(currentDir, package, version);

                                break;

                            case "repo":
                                string repoPath = null;

                                if (args.Length > (i + 1))
                                {
                                    repoPath = args[++i];
                                }

                                if (string.IsNullOrWhiteSpace(repoPath))
                                {
                                    throw new Exception("Please define repo path.");
                                }

                                if (!Path.IsPathRooted(repoPath))
                                {
                                    repoPath = Path.Combine(currentDir, repoPath);
                                }

                                string repoName = null;

                                if (args.Length > (i + 2) && (args[i + 1] == "-n" || args[i + 1] == "--name"))
                                {
                                    repoName = args[i + 2];
                                }
                                
                                AddRepo(repoPath, repoName);
                                Console.WriteLine("done");
                                break;

                            default:
                                throw new Exception($"{entity} is unknown entity for adding.");
                        }

                        skip = true;

                        return bridgeOptions;

                    case "remove":
                        string r_entity = null;

                        if (args.Length > (i + 1))
                        {
                            r_entity = args[++i];
                        }

                        switch (r_entity)
                        {
                            case "package":
                                string package = null;

                                if (args.Length > (i + 1))
                                {
                                    package = args[++i];
                                }

                                if (string.IsNullOrWhiteSpace(package))
                                {
                                    throw new Exception("Please define package name.");
                                }

                                RemovePackage(currentDir, package);

                                break;

                            default:
                                throw new Exception($"{r_entity} is unknown entity for remove.");
                        }

                        skip = true;

                        return bridgeOptions;

                    case "restore":
                        RestorePackages(currentDir);
                        skip = true;

                        return bridgeOptions;
                    case "build":
                        bridgeOptions.Rebuild = true;
                        bridgeOptions.Folder = currentDir;
                        RestorePackages(currentDir);

                        break;

                    case "run":
                        bridgeOptions.Folder = currentDir;
                        RestorePackages(currentDir);
                        run = true;

                        break;

                    case "new":
                        string tpl = "classlib";
                        string arg = null;
                        skip = true;

                        if (args.Length > (i + 1))
                        {
                            arg = args[++i];

                            switch(arg)
                            {
                                case "-h":
                                case "--help":
                                    ShowNewCommandHelp();

                                    return bridgeOptions;
                                case "-l":
                                case "--list":
                                    ShowTemplatesList();

                                    return bridgeOptions;
                                case "-i":
                                case "--install":
                                    if (args.Length <= (i + 1))
                                    {
                                        Error("Please define path to new template");

                                        return bridgeOptions;
                                    }

                                    InstallTemplate(args[++i]);

                                    return bridgeOptions;
                                case "-u":
                                case "--uninstall":
                                    if (args.Length <= (i + 1))
                                    {
                                        Error("Please define template's name to uninstall");

                                        return bridgeOptions;
                                    }

                                    UninstallTemplate(args[++i]);

                                    return bridgeOptions;
                                default:
                                    tpl = arg;

                                    break;
                            }                            
                        }

                        CreateProject(currentDir, tpl);

                        return bridgeOptions;

                    case "-p":
                    case "--project":
                        if (bridgeOptions.Lib != null)
                        {
                            Error("Error: Project and assembly file specification is mutually exclusive.");

                            return null;
                        };

                        bridgeOptions.ProjectLocation = args[++i];

                        break;

                    case "-b":
                    case "--bridge":
                        bridgeOptions.BridgeLocation = args[++i];

                        break;

                    case "-o":
                    case "--output":
                        bridgeOptions.OutputLocation = args[++i];

                        break;

                    case "-c":
                    case "--configuration":
                        configuration = args[++i];
                        hasPriorityConfiguration = true;

                        break;

                    case "-P":
                    case "--platform":
                        platform = args[++i];
                        hasPriorityPlatform = true;

                        break;

                    case "-D":
                    case "--define":
                        defineConstants = args[++i];
                        hasPriorityDefineConstants = true;

                        break;

                    case "-r":
                    case "--rebuild":
                        bridgeOptions.Rebuild = true;

                        break;

                    case "--nocore":
                        bridgeOptions.ExtractCore = false;

                        break;

                    case "-s":
                    case "--source":
                        bridgeOptions.Sources = args[++i];

                        break;

                    case "-S":
                    case "--settings":
                        if (args.Length > (i + 1))
                        {
                            var next = args[i + 1];

                            if (next == "-h" || next == "-help")
                            {
                                i++;
                                ShowSettingsHelp();
                                skip = true;

                                return bridgeOptions; // success. Asked for help. Help provided
                            }
                        }
                        

                        var error = args.Length > (i + 1) ? ParseProjectProperties(bridgeOptions, args[++i]) : "No settings are provided";

                        if (error != null)
                        {
                            Error("Invalid argument --setting(-S): " + args[i]);
                            Error(error);

                            return null;
                        }

                        break;

                    case "-f":
                    case "--folder":
                        bridgeOptions.Folder = Path.Combine(currentDir, args[++i]);

                        break;

                    case "--referencespath":
                        EnsureProperty(bridgeOptions, "ReferencesPath");
                        bridgeOptions.ReferencesPath = args[++i];
                        bridgeOptions.ReferencesPath = Path.IsPathRooted(bridgeOptions.ReferencesPath) ? bridgeOptions.ReferencesPath : Path.Combine(currentDir, bridgeOptions.ReferencesPath);

                        break;

                    case "-R":
                    case "--recursive":
                        bridgeOptions.Recursive = true;

                        break;

                    case "--norecursive":
                        bridgeOptions.Recursive = false;

                        break;

                    case "-h":
                    case "--help":
                        ShowHelp();
                        skip = true;

                        return bridgeOptions; // success. Asked for help. Help provided

                    case "-v":
                    case "--version":
                        ShowVersion();
                        skip = true;

                        return bridgeOptions; // success. Asked for version. Version provided.

                    case "--notimestamp":
                        bridgeOptions.NoTimeStamp = true;

                        break;

#if DEBUG
                    case "-d":
                    case "--debug":
                    case "--attachdebugger":
                        System.Diagnostics.Debugger.Launch();
                        break;
#endif
                    case "--": // stop reading commandline arguments
                        // Only non-hyphen commandline argument accepted is the file name of the project or
                        // assembly file, so if not provided already, when this option is specified, check if
                        // it is still needed and bind the file to the correct location
                        if (i < (args.Length - 1))
                        {
                            // don't care about success. If not set already, then try next cmdline argument
                            // as the file parameter and ignore following arguments, if any.
                            BindCmdArgumentToOption(args[i + 1], bridgeOptions);
                        }

                        i = args.Length; // move to the end of arguments list

                        break;

                    default:

                        // If this argument does not look like a cmdline switch and
                        // neither backwards -project nor -lib were specified
                        if (!BindCmdArgumentToOption(args[i], bridgeOptions))
                        {
                            Error("Invalid argument: " + args[i]);

                            return null;
                        }

                        break;
                }

                i++;
            }

            if (hasPriorityConfiguration)
            {
                bridgeOptions.ProjectProperties.Configuration = configuration;
            }

            if (hasPriorityPlatform)
            {
                bridgeOptions.ProjectProperties.Platform = platform;
            }

            if (hasPriorityDefineConstants)
            {
                bridgeOptions.ProjectProperties.DefineConstants = defineConstants;
            }

            if (bridgeOptions.ProjectLocation == null && bridgeOptions.Lib == null)
            {
                var folder = bridgeOptions.Folder ?? currentDir;

                var csprojs = new string[] { };

                try
                {
                    csprojs = Directory.GetFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Error(ex.ToString());
                }

                if (csprojs.Length > 1)
                {
                    Error("Could not default to a csproj because multiple were found:");
                    Info(string.Join(", ", csprojs.Select(path => Path.GetFileName(path))));

                    return null; // error: arguments not provided, so can't guess what to do
                }

                if (csprojs.Length > 0)
                {
                    var csproj = csprojs[0];

                    bridgeOptions.ProjectLocation = csproj;
                    Info("Defaulting Project Location to " + csproj);
                }
            }

            if (string.IsNullOrEmpty(bridgeOptions.OutputLocation))
            {
                bridgeOptions.OutputLocation = !string.IsNullOrWhiteSpace(bridgeOptions.ProjectLocation)
                    ? Path.GetFileNameWithoutExtension(bridgeOptions.ProjectLocation) : bridgeOptions.Folder;
            }

            if (bridgeOptions.IsFolderMode)
            {
                if (string.IsNullOrEmpty(bridgeOptions.Lib) && IsPropertyExist(bridgeOptions, "ReferencesPath"))
                {
                    var folder = bridgeOptions.Folder ?? currentDir;

                    if (!string.IsNullOrWhiteSpace(bridgeOptions.ReferencesPath))
                    {
                        bridgeOptions.Lib = Path.Combine(Path.IsPathRooted(bridgeOptions.ReferencesPath) ? bridgeOptions.ReferencesPath : Path.Combine(folder, bridgeOptions.ReferencesPath), new DirectoryInfo(folder).Name + ".dll");
                    }
                    else
                    {
                        if (!TryReadReferencesPathFromConfig(folder, bridgeOptions))
                        {
                            bool found = false;
                            string name = new DirectoryInfo(folder).Name;

                            foreach (var path in DEFAULT_REFERENCES_PATHES)
                            {
                                var checkFolder = Path.Combine(folder, path);

                                if (Directory.Exists(checkFolder))
                                {
                                    bridgeOptions.ReferencesPath = checkFolder;
                                    folder = checkFolder;
                                    found = true;

                                    break;
                                }
                            }

                            bridgeOptions.Lib = Path.Combine(found ? folder : Path.Combine(folder, DEFAULT_REFERENCES_PATHES[0]), name + ".dll");
                        }
                    }
                }

                bridgeOptions.DefaultFileName = Path.GetFileNameWithoutExtension(bridgeOptions.Lib);
                bridgeOptions.ProjectProperties.AssemblyName = bridgeOptions.DefaultFileName;
            }
            else
            {
                bridgeOptions.DefaultFileName = Path.GetFileName(bridgeOptions.OutputLocation);
            }

            if (string.IsNullOrWhiteSpace(bridgeOptions.DefaultFileName))
            {
                bridgeOptions.DefaultFileName = Path.GetFileName(bridgeOptions.OutputLocation);
            }

            return bridgeOptions;
        }

        private static void ShowNewCommandHelp()
        {
            Console.WriteLine(@"Usage: 
  bridge new [options]


Options:
  -h, --help                 Displays help for this command.
  -l, --list                 Lists all templates.
  -i, --install              Installs a template.
  -u, --uninstall            Uninstalls a template.


Templates          Short Name          Description
----------------------------------------------------------------------------
Class Library      classlib            New C# Class Library project.


Examples:
    bridge new               New project using the default template.
    bridge new classlib      New project using classlib template.
    bridge new --help        Display help.");
        }

        private static void ShowSettingsHelp()
        {
            Console.WriteLine(@"Accepted Names:
    AssemblyName
    CheckForOverflowUnderflow
    Configuration
    DefineConstants
    OutputPath
    OutDir
    OutputType
    Platform
    RootNamespace

  Example: 
    -S name1: value1,name2: value2

  Note:
    Options - c, -P and - D have priority over - S");
        }

        private static bool IsPropertyExist(dynamic obj, string name)
        {
            if (obj is System.Dynamic.ExpandoObject)
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return obj.GetType().GetProperty(name) != null;
        }

        private static void EnsureProperty(dynamic obj, string name)
        {
            if (!IsPropertyExist(obj, name))
            {
                throw new InvalidOperationException($"'{name}' property doesn't exist in '{obj.GetType().FullName}'. Core version: {GetCompilerVersion().ToString()}");
            }
        }

        private static string ParseProjectProperties(dynamic bridgeOptions, string parameters)
        {
            dynamic properties = CreateProjectProperties();
            bridgeOptions.ProjectProperties = properties;

            if (string.IsNullOrWhiteSpace(parameters))
            {
                return null;
            }

            if (parameters != null && parameters.Length > 1 && parameters[0] == '"' && parameters.Last() == '"')
            {
                parameters = parameters.Trim('"');
            }

            var settings = new Dictionary<string, string>();

            var splitParameters = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in splitParameters)
            {
                if (pair == null)
                {
                    continue;
                }

                var parts = pair.Split(new char[] { ':' }, 2);

                if (parts.Length < 2)
                {
                    continue;
                }

                var name = parts[0].Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                string value;

                if (settings.ContainsKey(name))
                {
                    value = settings[name];

                    continue;
                }

                value = parts[1];

                if (value != null && value.Length > 1 && (value[0] == '"' || value.Last() == '"'))
                {
                    value = value.Trim('"');
                }

                settings.Add(name, value);
            }

            try
            {
                properties.SetValues(settings);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }

            return null;
        }
    }
}