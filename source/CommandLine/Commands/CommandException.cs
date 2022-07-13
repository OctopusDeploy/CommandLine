using System;

namespace Octopus.CommandLine.Commands;

public class CommandException : Exception
{
    public CommandException(string message)
        : base(message)
    {
    }
}