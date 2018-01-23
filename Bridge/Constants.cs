using System.Reflection;

namespace Bridge.CLI
{
    /// <summary>
    /// Constant values valid throughout the application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Directory for CLI to look for initial assemblies on an empty project folder.
        /// </summary>
        public static readonly string AssembliesFolder = "tools";

        /// <summary>
        /// Directory for CLI to look for Bridge core library (Bridge.dll).
        /// </summary>
        public static readonly string BridgeCoreFolder = "lib";

        /// <summary>
        /// Directory for CLI to look for template projects for new projects installation.
        /// </summary>
        public static readonly string TemplatesFolder = "templates";

        public static readonly string RepoList = "repos.config";

        /// <summary>
        /// Gets just major+minor (.0) version of Bridge reflected in Bridge CLI.
        /// </summary>
        public static readonly string MinBridgeVersion = Assembly.GetEntryAssembly().GetName().Version.Major + "." + Assembly.GetEntryAssembly().GetName().Version.Minor + ".0";
    }
}