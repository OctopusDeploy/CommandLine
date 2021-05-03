using System;
using System.Reflection;
using Newtonsoft.Json;
using Serilog;

namespace Octopus.CommandLine
{
    public class DefaultCommandOutputProvider : CommandOutputProviderBase
    {
        public DefaultCommandOutputProvider(ILogger logger) : base(logger)
        {
        }

        protected override string GetAppVersion()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
                throw new ApplicationException("Unable to determine entry assembly");
            var assemblyInformationalVersionAttribute = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (assemblyInformationalVersionAttribute != null)
                return assemblyInformationalVersionAttribute.InformationalVersion;
            return entryAssembly.GetName().Version.ToString();
        }

        protected override string SerializeObjectToJson(object o)
            => JsonConvert.SerializeObject(o);
    }
}
