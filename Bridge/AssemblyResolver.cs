using System;
using System.Reflection;

namespace Bridge.CLI
{
    public class AssemblyResolver
    {
        public string CoreFolder { get; private set; }

        public AssemblyResolver(string coreFolder)
        {
            this.CoreFolder = coreFolder;
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var domain = sender as AppDomain;

            AssemblyName askedAssembly = new AssemblyName(args.Name);
            Assembly assemblyLoaded = null;

            assemblyLoaded = AssemblyResolver.CheckIfAssemblyLoaded(askedAssembly.Name, domain);

            if (assemblyLoaded != null)
            {
                return assemblyLoaded;
            }

            var asmFile = System.IO.Path.Combine(CoreFolder, askedAssembly.Name + ".dll");

            if (System.IO.File.Exists(asmFile))
            {
                return System.Reflection.Assembly.LoadFile(asmFile);
            }

            return null;
        }

        public static Assembly CheckIfAssemblyLoaded(string fullAssemblyName, AppDomain domain)
        {
            var assemblies = domain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var assemblyName = new AssemblyName(assembly.FullName);

                if (assemblyName.Name == fullAssemblyName)
                {
                    return assembly;
                }
            }

            return null;
        }

        public static Assembly CheckIfFullAssemblyLoaded(AssemblyName name, AppDomain domain)
        {
            var assemblies = domain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var assemblyName = new AssemblyName(assembly.FullName);

                if (assemblyName.FullName == name.FullName)
                {
                    return assembly;
                }
            }

            return null;
        }
    }
}