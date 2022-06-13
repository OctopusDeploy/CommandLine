using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.ShellCompletion;

namespace Octopus.CommandLine.Commands;

[Command("install-autocomplete", Description = "Install a shell auto-complete script into your shell profile, if they aren't already there. Supports pwsh, zsh, bash & powershell.")]
public class InstallAutoCompleteCommand : CommandBase
{
    readonly IShellCompletionInstaller[] installers;
    readonly string supportedShells;

    public InstallAutoCompleteCommand(ICommandOutputProvider commandOutputProvider, IEnumerable<IShellCompletionInstaller> installers) : base(commandOutputProvider)
    {
        this.installers = installers.ToArray();
        if (!this.installers.Any())
        {
            var message = $"Error: No shell completion installers found. Auto-complete installation is unavailable.{Environment.NewLine}" +
                $"Developers - please ensure that all implementations of {nameof(IShellCompletionInstaller)} within the CommandLine library are registered.";
            throw new CommandException(message);
        }

        var options = Options.For("Install AutoComplete");
        supportedShells = this.installers.Select(x => x.SupportedShell.ToString()).ReadableJoin();

        options.Add<SupportedShell>("shell=",
            $"The type of shell to install auto-complete scripts for. This will alter your shell configuration files. Supported shells are {supportedShells}.",
            v => { ShellSelection = v; });
        options.Add<bool>("dryRun",
            "[Optional] Dry run will output the proposed changes to console, instead of writing to disk.",
            v => DryRun = true);
    }

    public bool DryRun { get; set; }

    public SupportedShell ShellSelection { get; set; }

    public override Task Execute(string[] commandLineArguments)
    {
        Options.Parse(commandLineArguments);
        if (printHelp)
        {
            GetHelp(Console.Out, commandLineArguments);
            return Task.FromResult(0);
        }

        var invalidShellSelectionMessage = $"Please specify the type of shell to install auto-completion for: --shell=XYZ. Valid values are {supportedShells}.";
        var installer = installers.FirstOrDefault(x => x.SupportedShell == ShellSelection);
        if (installer == null) throw new CommandException(invalidShellSelectionMessage);

        commandOutputProvider.PrintHeader();
        if (DryRun) commandOutputProvider.Warning("DRY RUN");
        commandOutputProvider.Information($"Installing auto-complete scripts for {ShellSelection}");
        installer.Install(DryRun);
        return Task.FromResult(0);
    }
}