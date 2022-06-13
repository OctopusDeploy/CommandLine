using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine;

public class CommandLocator : ICommandLocator
{
    readonly IEnumerable<ICommand> commands;

    public CommandLocator(IEnumerable<ICommand> commands)
        => this.commands = commands;

    public ICommandMetadata[] List()
    {
        return commands
            .Where(t => t.CommandMetadata != null)
            .Select(t => t.CommandMetadata)
            .ToArray();
    }

    public ICommand Find(string name)
    {
        name = name.Trim().ToLowerInvariant();

        var found = commands
            .Where(command => command.CommandMetadata != null)
            .FirstOrDefault(command => command.CommandMetadata.Name == name || command.CommandMetadata.Aliases.Any(a => a == name));

        return found;
    }

    public ICommand GetCommand(string[] args)
    {
        var first = GetFirstArgument(args);

        if (string.IsNullOrWhiteSpace(first))
            // if help is unavailable, it means the registration of commands is messed up, and HelpCommand wasn't available
            return Find("help") ?? throw new CommandException($"Error: No 'help' command found. Help is unavailable.{Environment.NewLine}"
                + $"Developers - please ensure that all implementations of {nameof(ICommand)} within the CommandLine library are registered.");

        var command = Find(first);
        if (command == null)
            throw new CommandException("Error: Unrecognized command '" + first + "'");

        return command;
    }

    static string GetFirstArgument(IEnumerable<string> args)
        => (args.FirstOrDefault() ?? string.Empty).ToLowerInvariant().TrimStart('-', '/');
}