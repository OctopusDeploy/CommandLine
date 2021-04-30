using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine.Extensions
{
    public static class CommandBaseExtensions
    {
        public static IEnumerable<string> GetOptionNames(this CommandBase command)
        {
            return command.Options.OptionSets
                .SelectMany(keyValuePair => keyValuePair.Value)
                .SelectMany(option => option.Names)
                .Select(str => $"--{str}");
        }
    }
}
