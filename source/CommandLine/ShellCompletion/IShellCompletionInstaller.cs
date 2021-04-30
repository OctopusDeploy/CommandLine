using System;
using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine.ShellCompletion
{
    public interface IShellCompletionInstaller
    {
        SupportedShell SupportedShell { get; }
        void Install(bool dryRun);
        string ProfileLocation { get; }
        string ProfileScript { get; }
    }
}