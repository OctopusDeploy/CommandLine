using System;
using Octopus.CommandLine.Commands;

namespace Tests.Helpers
{
    public static class TestCommandExtensions
    {
        public static void Execute(this ICommand command, params string[] args)
        {
            command.Execute(args).GetAwaiter().GetResult();
        }
    }
}
