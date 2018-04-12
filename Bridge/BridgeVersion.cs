using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.CLI
{
    public class BridgeVersion
    {
        private static string nl = Environment.NewLine;

        public static InformationalVersion GetCompilerInformationalVersion()
        {
            return new InformationalVersion(System.Diagnostics.FileVersionInfo.GetVersionInfo(Program.TranslatorAssembly.Location).ProductVersion);
        }

        /// <summary>
        /// This just throws exceptions if some package sanity checks do not pass.
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="repoPath"></param>
        /// <param name="version"></param>
        public static void ValidatePackageVersion(string packageName, string repoPath, string version)
        {
            ValidatePackageVersion(packageName, repoPath, new InformationalVersion(version));
        }

        /// <summary>
        /// This just throws exceptions if some package sanity checks do not pass.
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="repoPath"></param>
        /// <param name="version"></param>
        public static void ValidatePackageVersion(string packageName, string repoPath, InformationalVersion version)
        {
            switch (packageName)
            {
                case string p when p == "Bridge" || p == "Bridge.Min" || p == "Bridge.Core":
                    if (!Program.EnablePrerelease && version.IsPrerelease)
                    {
                        throw new Exception("Invalid package version: " + version + "." + nl +
                            "Prerelease packages handling is disabled. If you want to enable it, please" + nl +
                            "specify the --enable-prerelease commandline argument.");
                    }

                    if (Constants.MinBridgeVersion > version)
                    {
                        throw new Exception("Latest available " + packageName + " version on repository is behind minimum required." + nl +
                            "Repository path: " + repoPath + nl +
                            "Latest available version: " + version + nl +
                            "Minimal version required: " + Constants.MinBridgeVersion);
                    }
                    break;
            }
        }
    }
}
