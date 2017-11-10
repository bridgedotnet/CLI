using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Bridge.CLI
{
    public partial class Program
    {
        private static System.Reflection.Assembly GetTranslatorAssembly(string coreDir)
        {
            return GetAssembly(Path.Combine(coreDir, "Bridge.Translator.dll"));
        }

        private static System.Reflection.Assembly GetContractAssembly(string coreDir)
        {
            return GetAssembly(Path.Combine(coreDir, "Bridge.Contract.dll"));
        }

        private static string GetBridgeLocation(string dir)
        {
            dir = dir ?? Environment.CurrentDirectory;
            var tmpDir = Path.Combine(dir, "packages");

            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.Combine(Directory.GetParent(dir).ToString(), "packages");
            }

            dir = tmpDir;

            string asmFile = null;
            string name = "Bridge.dll";

            if (dir != null && Directory.Exists(dir))
            {
                var packagesDirs = Directory.GetDirectories(dir, "bridge.core.*", SearchOption.TopDirectoryOnly);

                if (packagesDirs.Length > 0)
                {
                    packagesDirs = SortNewestPackage("bridge.core", packagesDirs);

                    foreach (var packageDir in packagesDirs)
                    {
                        if (Regex.IsMatch(Path.GetFileName(packageDir), @"\ABridge\.Core\.\d+(?:\.\d+)+\z", RegexOptions.IgnoreCase))
                        {
                            asmFile = Path.Combine(Path.Combine(Path.Combine(packageDir, "lib"), "net40"), name);

                            if (File.Exists(asmFile))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (asmFile == null)
            {
                asmFile = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Constants.BridgeCoreFolder), name);
            }

            return File.Exists(asmFile) ? asmFile : null;
        }

        private static System.Reflection.Assembly GetAssembly(string name)
        {
            return System.Reflection.Assembly.LoadFile(name);
        }

        private static string GetCoreFolder(string dir = null)
        {
            dir = dir ?? Environment.CurrentDirectory;
            var tmpDir = Path.Combine(dir, "packages");

            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.Combine(Directory.GetParent(dir).ToString(), "packages");
            }

            dir = tmpDir;

            string path = null;

            if (dir != null && Directory.Exists(dir))
            {
                var packagesDirs = Directory.GetDirectories(dir, "bridge.min.*", SearchOption.TopDirectoryOnly);

                if (packagesDirs.Length > 0)
                {
                    packagesDirs = SortNewestPackage("bridge.min", packagesDirs);

                    foreach (var packageDir in packagesDirs)
                    {
                        if (Regex.IsMatch(Path.GetFileName(packageDir), @"\ABridge\.Min\.\d+(?:\.\d+)+\z", RegexOptions.IgnoreCase))
                        {
                            path = Path.Combine(packageDir, Constants.AssembliesFolder);
                            var asmFile = Path.Combine(path, "Bridge.Translator.dll");

                            if (File.Exists(asmFile))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (path == null || !Directory.Exists(path))
            {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Constants.AssembliesFolder);
            }

            return path;
        }
    }
}