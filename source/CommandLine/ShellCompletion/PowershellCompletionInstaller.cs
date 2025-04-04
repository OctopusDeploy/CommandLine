using System;
using System.IO;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion;

public class PowershellCompletionInstaller : PowershellCompletionInstallerBase
{
    public PowershellCompletionInstaller(ICommandOutputProvider commandOutputProvider)
        : this(commandOutputProvider, new OctopusFileSystem(), new[] { AssemblyHelper.GetExecutablePath() })
    {
    }

    public PowershellCompletionInstaller(ICommandOutputProvider commandOutputProvider, string[] executablePaths)
        : this(commandOutputProvider, new OctopusFileSystem(), executablePaths)
    {
    }

    public PowershellCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executablePaths)
        : base(commandOutputProvider, fileSystem, executablePaths)
    {
    }

    public override SupportedShell SupportedShell => SupportedShell.Powershell;

    static string WindowsPowershellConfigLocation => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "WindowsPowershell"
    );

    public override string ProfileLocation => Path.Combine(WindowsPowershellConfigLocation, PowershellProfileFilename);
    public override string ProfileScript => base.ProfileScript.NormalizeNewLinesForWindows();

    public override void Install(bool dryRun)
    {
        if (ExecutionEnvironment.IsRunningOnNix || ExecutionEnvironment.IsRunningOnMac)
            throw new CommandException("Unable to install for powershell on non-windows platforms. Please use --shell=pwsh instead.");
        base.Install(dryRun);
    }
}