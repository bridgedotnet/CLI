using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Bridge.CLI
{
    public partial class Program
    {
        private static void InstallTemplate(string path)
        {
            var rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var templatesPath = Path.Combine(rootPath, Constants.TemplatesFolder);

            if (File.Exists(path))
            {
                try
                {
                    ZipFile.ExtractToDirectory(path, templatesPath);
                }
                catch (Exception e)
                {
                    Error($"The error during template's archive extraction: {e.Message}");
                    return;
                }
            }
            else
            {
                var isFile = true;
                try
                {
                    var uri = new Uri(path);
                    isFile = uri.IsFile;
                }
                catch (Exception)
                {
                }

                if (isFile)
                {
                    Error("Template file doesn't exist");
                    return;
                }

                WriteLine("Downloading template ", false);
                using (var spinner = new ConsoleSpinner())
                {
                    spinner.Start();

                    try
                    {
                        var localFile = Path.GetTempFileName();
                        WebClient client = new WebClient();
                        client.DownloadFile(path, localFile);
                        client.Dispose();
                        ZipFile.ExtractToDirectory(localFile, templatesPath);
                        File.Delete(localFile);
                        WriteLine("done");
                        return;
                    }
                    catch (Exception e)
                    {
                        Error($"The download error: {e.Message}");
                        return;
                    }                    
                }
            }            

            WriteLine($"Template has been installed");
        }

        private static void UninstallTemplate(string name)
        {
            var rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var templatesPath = Path.Combine(rootPath, Constants.TemplatesFolder);
            var templatePath = Path.Combine(templatesPath, name);

            if (Directory.Exists(templatePath))
            {
                try
                {
                    Directory.Delete(templatePath, true);
                    WriteLine($"Template {name} has been uninstalled");
                }
                catch (Exception e)
                {
                    Error($"Template {name} was not uninstalled. Reason: {e.Message}");
                }                
            }
            else
            {
                Error($"Template with name {name} doesn't exist");
            }
        }

        private static void ShowTemplatesList()
        {
            var rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var templatesPath = Path.Combine(rootPath, Constants.TemplatesFolder);

            if (Directory.Exists(templatesPath))
            {
                var tpls = Directory.GetDirectories(templatesPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var tpl in tpls)
                {
                    WriteLine(Path.GetFileName(tpl));
                }
            }
        }

        private static void CreateProject(string folder, string template)
        {
            var rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var templatesPath = Path.Combine(rootPath, Constants.TemplatesFolder);
            var templatePath = Path.Combine(templatesPath, template);

            if (Directory.Exists(templatePath))
            {
                foreach (string dirPath in Directory.GetDirectories(templatePath, "*", SearchOption.AllDirectories))
                {
                    var path = dirPath.Replace(templatePath, folder);

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                foreach (string newPath in Directory.GetFiles(templatePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(templatePath, folder), true);
                }

                var packagesConfigPath = Path.Combine(folder, "packages.config");

                if (File.Exists(packagesConfigPath))
                {
                    XDocument config = XDocument.Load(packagesConfigPath);

                    var packages = config
                        .Element("packages")
                        .Elements("package")
                        .Select(packageElem => new { id = packageElem.Attribute("id").Value, version = packageElem.Attribute("version")?.Value })
                        .ToList();

                    foreach (var pkg in packages)
                    {
                        AddPackage(folder, pkg.id, pkg.version);
                    }
                }
            }
        }

        private static void CreatePackageConfig(string folder)
        {
            var fileName = Path.Combine(folder, "packages.config");

            if (!File.Exists(fileName))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<packages>");
                sb.AppendLine("</packages>");

                using (var stream = File.CreateText(fileName))
                {
                    stream.Write(sb.ToString());
                }
            }
        }

        private static void AddPackage(string folder, string packageName, string version = null, bool restore = false)
        {
            var packagesFolder = Path.Combine(folder, "packages");

            if (!Directory.Exists(packagesFolder))
            {
                Directory.CreateDirectory(packagesFolder);
            }

            bool hasVersion = !string.IsNullOrWhiteSpace(version);
            string uri = "https://www.nuget.org/api/v2/package/" + packageName + (hasVersion ? "/" + version : "");
            string name = packageName + (hasVersion ? "." + version : "");
            string localFile = Path.Combine(packagesFolder, name + ".nupkg");

            if (File.Exists(localFile))
            {
                File.Delete(localFile);
            }

            Console.WriteLine();
            var msg = $"Installing {packageName}";
            
            if(msg.Length >= 26)
            {
                msg += "  ";
            }
            else
            {
                msg = msg.PadRight(26);
            }

            Console.Write(msg);

            if (Directory.Exists(Path.Combine(packagesFolder, name)))
            {
                Warn("skipped (already exists)");

                return;
            }

            try
            {
                string packageFolder = null;
                bool exists = false;

                using (var spinner = new ConsoleSpinner())
                {
                    spinner.Start();

                    if (hasVersion)
                    {
                        name = packageName + "." + version;
                    }
                    else
                    {
                        var webRequest = (HttpWebRequest)WebRequest.Create(uri + "?" + new Random().Next());
                        webRequest.AllowAutoRedirect = false;
                        var webResponse = (HttpWebResponse)webRequest.GetResponse();

                        if (!String.IsNullOrEmpty(webResponse.Headers["Location"]))
                        {
                            var fileName = System.IO.Path.GetFileName(webResponse.Headers["Location"]);
                            name = Path.GetFileNameWithoutExtension(fileName);
                        }

                        webResponse.Dispose();
                    }

                    name = name[0].ToString().ToUpper() + name.Substring(1);
                    packageFolder = Path.Combine(packagesFolder, name);

                    if (Directory.Exists(packageFolder))
                    {
                        exists = true;
                    }
                    else
                    {
                        WebClient client = new WebClient();
                        client.DownloadFile(uri, localFile);
                        client.Dispose();
                    }
                }

                if (exists)
                {
                    Warn("skipped (already exists)");
                }
                else
                {
                    Info("done");

                    if (!restore)
                    {
                        RemovePackage(folder, packageName);
                    }

                    Directory.CreateDirectory(packageFolder);
                    ZipFile.ExtractToDirectory(localFile, packageFolder);
                    File.Move(localFile, Path.Combine(packageFolder, Path.GetFileName(packageFolder) + ".nupkg"));

                    PackageInfo info = null;

                    if (!restore)
                    {
                        info = ReadPackageInfo(packageName, packageFolder);
                        AddPackageToConfig(folder, info.Id, info.Version);
                    }

                    CleanPackageAfterDownload(packageFolder);

                    if (info != null && !restore)
                    {
                        DownloadDependencies(folder, info);
                    }
                }
            }
            catch (Exception e)
            {
                Error("Error: ");
                Error(e.Message);

                Console.WriteLine();
            }
        }

        private static void CleanPackageAfterDownload(string packageFolder)
        {
            string[] dirs = new string[] { "_rels", "package" };
            string[] files = new string[] { "[Content_Types].xml", "*.nuspec" };

            foreach (var dir in dirs)
            {
                Directory.Delete(Path.Combine(packageFolder, dir), true);
            }

            foreach (var file in files)
            {
                var foundFiles = Directory.GetFiles(packageFolder, file, SearchOption.TopDirectoryOnly);

                foreach (var foundFile in foundFiles)
                {
                    File.Delete(foundFile);
                }
            }
        }

        private static string PackageExists(string folder, string id, string version)
        {
            var tmpDir = Path.Combine(folder, "packages");

            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.Combine(Directory.GetParent(folder).ToString(), "packages");
            }

            folder = tmpDir;

            var name = id + "." + version;
            var packageFolder = Path.Combine(folder, name);

            if (Directory.Exists(packageFolder))
            {
                return packageFolder;
            }

            var dirs = Directory.GetDirectories(folder, id + ".*", SearchOption.TopDirectoryOnly);

            if (dirs.Length > 0)
            {
                dirs = SortNewestPackage(id, dirs);

                var dir = dirs.First();
                var dirName = Path.GetFileName(dir);
                var dirVersion = dirName.Substring(id.Length + 1);

                if (new Version(version).CompareTo(new Version(dirVersion)) > 0)
                {
                    RemovePackage(folder, id);
                    return null;
                }
                else
                {
                    return dir;
                }
            }

            return null;
        }

        private static void RemovePackage(string folder, string id)
        {
            var packagesConfig = Path.Combine(folder, "packages.config");

            var tmpDir = Path.Combine(folder, "packages");

            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.Combine(Directory.GetParent(folder).ToString(), "packages");
            }

            folder = tmpDir;

            var dirs = Directory.GetDirectories(folder, id + ".*", SearchOption.TopDirectoryOnly);

            if (dirs.Length > 0)
            {
                foreach (var dir in dirs)
                {
                    Directory.Delete(dir, true);
                }
            }

            if (File.Exists(packagesConfig))
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(File.ReadAllText(packagesConfig));

                var nodes = doc.DocumentElement.SelectNodes($"descendant::package[@id='{id}']");

                if (nodes.Count > 0)
                {
                    foreach (System.Xml.XmlNode node in nodes)
                    {
                        node.ParentNode.RemoveChild(node);
                    }

                    doc.Save(packagesConfig);
                }
            }
        }

        private static void DownloadDependencies(string folder, PackageInfo info)
        {
            if (info.Dependecies?.Length > 0)
            {
                foreach (var dependency in info.Dependecies)
                {
                    if (PackageExists(folder, dependency.Id, dependency.Version) == null)
                    {
                        AddPackage(folder, dependency.Id, dependency.Version);
                    }
                }
            }
        }

        private static PackageInfo ReadPackageInfo(string id, string packageFolder)
        {
            var nuspec = Directory.GetFiles(packageFolder, "*.nuspec").FirstOrDefault();

            if (nuspec != null)
            {
                XDocument config = XDocument.Load(nuspec);

                var ns = config.Root.Name.Namespace.ToString();
                var meta = config.Element(XName.Get("package", ns)).Element(XName.Get("metadata", ns));
                var id_package = meta.Element(XName.Get("id", ns)).Value;
                var version = meta.Element(XName.Get("version", ns)).Value;
                var info = new PackageInfo(id_package, version);

                var dependencies = meta.Element(XName.Get("dependencies", ns));

                if (dependencies != null)
                {
                    info.Dependecies = dependencies.Elements(XName.Get("dependency", ns)).Select(d => new PackageInfo(d.Attribute("id").Value, d.Attribute("version").Value)).ToArray();
                }

                return info;
            }

            var name = Path.GetFileName(packageFolder);

            return new PackageInfo(id, name.Substring(id.Length + 1));
        }

        private static void AddPackageToConfig(string folder, string id, string version, string targetFramework = null)
        {
            CreatePackageConfig(folder);

            var configFileName = Path.Combine(folder, "packages.config");
            var doc = new System.Xml.XmlDocument();

            doc.LoadXml(File.ReadAllText(configFileName));

            if (doc.DocumentElement.SelectSingleNode($"descendant::package[@id='{id}' and @version='{version}']") != null)
            {
                return;
            }

            var node = doc.CreateNode(System.Xml.XmlNodeType.Element, "package", null);
            var attr = doc.CreateAttribute("id");
            attr.Value = id;
            node.Attributes.Append(attr);

            attr = doc.CreateAttribute("version");
            attr.Value = version;
            node.Attributes.Append(attr);

            if (!string.IsNullOrWhiteSpace(targetFramework))
            {
                attr = doc.CreateAttribute("targetFramework");
                attr.Value = targetFramework;
                node.Attributes.Append(attr);
            }

            doc.SelectSingleNode("packages").AppendChild(node);
            doc.Save(configFileName);
        }

        private static string[] SortNewestPackage(string id, IEnumerable<string> dirs)
        {
            var versions = dirs.Select(d =>
            {
                var name = Path.GetFileName(d);
                return new Tuple<string, Version>(d, new Version(name.Substring(id.Length + 1)));
            }).ToList();

            versions.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            versions.Reverse();

            return versions.Select(v => v.Item1).ToArray();
        }

        private static void RestorePackages(string folder)
        {
            var packagesConfig = Path.Combine(folder, "packages.config");

            var tmpDir = Path.Combine(folder, "packages");

            if (!Directory.Exists(tmpDir))
            {
                tmpDir = Path.Combine(Directory.GetParent(folder).ToString(), "packages");
            }

            string packagesFolder = tmpDir;

            if (File.Exists(packagesConfig))
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(File.ReadAllText(packagesConfig));

                var nodes = doc.DocumentElement.SelectNodes($"descendant::package");

                if (nodes.Count > 0)
                {
                    foreach (System.Xml.XmlNode node in nodes)
                    {
                        string id = node.Attributes["id"].Value;
                        var versionAttr = node.Attributes["version"];
                        string version = versionAttr?.Value;
                        bool restore = false;

                        if (version == null)
                        {
                            restore = true;
                        }
                        else
                        {
                            string packageDir = Path.Combine(packagesFolder, id + "." + version);

                            if (!Directory.Exists(packageDir))
                            {
                                restore = true;
                            }
                        }                        

                        if (restore)
                        {
                            AddPackage(folder, id, version, true);
                        }
                    }
                }
            }
        }
    }
}

public class PackageInfo
{
    public PackageInfo(string id, string version)
    {
        Id = id;
        Version = version;
    }

    public string Id
    {
        get;
        set;
    }

    public string Version
    {
        get;
        set;
    }

    public PackageInfo[] Dependecies
    {
        get;
        set;
    }
}