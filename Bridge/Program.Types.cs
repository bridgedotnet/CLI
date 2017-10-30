using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bridge.CLI
{
    public partial class Program
    {
        private static Type GetEmitterException()
        {
            return ContractAssembly.GetTypes().First(t => t.Name == "EmitterException");
        }
        
        private static dynamic CreateBridgeOptions()
        {
            var type = TranslatorAssembly.GetTypes().First(t => t.Name == "BridgeOptions");
            return System.Activator.CreateInstance(type);
        }

        private static dynamic CreateProjectProperties()
        {
            var type = ContractAssembly.GetTypes().First(t => t.Name == "ProjectProperties");
            return System.Activator.CreateInstance(type);
        }

        private static dynamic CreateLogger()
        {
            var types = TranslatorAssembly.GetTypes();
            var loggerLevelType = ContractAssembly.GetTypes().First(t => t.Name == "LoggerLevel");
            var loggerLevelValue = Enum.ToObject(loggerLevelType, 10);
            var consoleLoggerType = types.First(t => t.FullName == "Bridge.Translator.Logging.ConsoleLoggerWriter");
            var fileLoggerType = types.First(t => t.FullName == "Bridge.Translator.Logging.FileLoggerWriter");

            var type = types.First(t => t.FullName == "Bridge.Translator.Logging.Logger");
            return System.Activator.CreateInstance(type, null, false, loggerLevelValue, true, System.Activator.CreateInstance(consoleLoggerType), System.Activator.CreateInstance(fileLoggerType));
        }

        private static dynamic CreateTranslatorProcessor(object bridgeOptions, object logger)
        {
            var processorType = TranslatorAssembly.GetTypes().First(t => t.Name == "TranslatorProcessor");
            return System.Activator.CreateInstance(processorType, bridgeOptions, logger);
        }

        private static bool TryReadReferencesPathFromConfig(string folder, dynamic bridgeOptions)
        {
            var type = ContractAssembly.GetTypes().First(t => t.Name == "ConfigHelper`1");
            var gtype = type.MakeGenericType(TranslatorAssembly.GetTypes().First(t => t.Name == "AssemblyInfo"));

            dynamic helper = System.Activator.CreateInstance(gtype, CreateLogger());
            var info = helper.ReadConfig("bridge.json", true, folder, bridgeOptions.ProjectProperties.Configuration);

            if (!string.IsNullOrWhiteSpace(info.ReferencesPath))
            {
                bridgeOptions.Lib = Path.Combine(Path.IsPathRooted(info.ReferencesPath) ? info.ReferencesPath : Path.Combine(folder, info.ReferencesPath), new DirectoryInfo(folder).Name + ".dll");
                return true;
            }

            return false;
        }
    }
}
