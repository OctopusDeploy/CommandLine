using System;
using Newtonsoft.Json;

namespace Octopus.CommandLine
{
    public interface ICommandOutputJsonSerializer
    {
        string SerializeObjectToJson(object o);
    }

    class DefaultCommandOutputJsonSerializer : ICommandOutputJsonSerializer
    {
        public string SerializeObjectToJson(object o) => JsonConvert.SerializeObject(o);
    }
}
