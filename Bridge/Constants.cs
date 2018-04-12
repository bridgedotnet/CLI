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

        private static InformationalVersion minBridgeVersion = null;

        /// <summary>
        /// Gets just major+minor (.0) version of Bridge reflected in Bridge CLI.
        /// If built against a prerelease version of Bridge, full version is required.
        /// </summary>
        public static InformationalVersion MinBridgeVersion
        {
            get
            {
                if (minBridgeVersion == null)
                {
                    var ver = new InformationalVersion(System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion);
                    minBridgeVersion = ver.IsPrerelease ? ver : new InformationalVersion(ver.Major, ver.Minor);
                }

                return minBridgeVersion;
            }
        }
    }
}