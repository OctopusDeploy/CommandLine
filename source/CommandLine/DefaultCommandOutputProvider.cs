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
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            return version;
        }

        protected override string SerializeObjectToJson(object o)
            => JsonConvert.SerializeObject(o);
    }
}
