using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine
{
    public class CommandLocator : ICommandLocator
    {
        private readonly IEnumerable<ICommand> commands;

        public CommandLocator(IEnumerable<ICommand> commands)
        {
            this.commands = commands;
        }

        public ICommandMetadata[] List()
        {
            return
                (from t in commands
                    let attribute =
                        (ICommandMetadata) t.GetType().GetTypeInfo().GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault()
                    where attribute != null
                    select attribute).ToArray();
        }

        public ICommand Find(string name)
        {
            name = name.Trim().ToLowerInvariant();

            var found = (from t in commands
                let attribute =
                    (ICommandMetadata) t.GetType().GetTypeInfo().GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault()
                where attribute != null
                where attribute.Name == name || attribute.Aliases.Any(a => a == name)
                select t).FirstOrDefault();

            return found;
        }

        public ICommand GetCommand(string[] args)
        {
            var first = GetFirstArgument(args);

            if (string.IsNullOrWhiteSpace(first))
                return Find("help");

            var command = Find(first);
            if (command == null)
                throw new CommandException("Error: Unrecognized command '" + first + "'");

            return command;
        }

        static string GetFirstArgument(IEnumerable<string> args)
        {
            return (args.FirstOrDefault() ?? string.Empty).ToLowerInvariant().TrimStart('-', '/');
        }
    }
}
