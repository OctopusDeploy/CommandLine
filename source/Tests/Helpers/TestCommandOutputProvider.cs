using System;
using Newtonsoft.Json;
using Octopus.CommandLine;
using Serilog;

namespace Tests.Helpers
{
    class TestCommandOutputProvider : CommandOutputProviderBase
    {
        public TestCommandOutputProvider(ILogger logger) : base(logger)
        {
        }

        protected override string GetAppVersion() => "1.0.0";

        protected override string SerializeObjectToJson(object o) => JsonConvert.SerializeObject(o);
    }
}
