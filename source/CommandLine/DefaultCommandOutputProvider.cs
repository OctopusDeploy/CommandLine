using System;
using System.Reflection;
using Newtonsoft.Json;
using Serilog;

namespace Octopus.CommandLine
{
    public class DefaultCommandOutputProvider : CommandOutputProviderBase
    {
        readonly string applicationName;

        public DefaultCommandOutputProvider(string applicationName, ILogger logger) : base(logger)
        {
            this.applicationName = applicationName;
        }

        protected override string GetAppName() => applicationName;

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
    }
}
