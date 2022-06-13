using System;
using System.IO;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion;

class PwshCompletionInstaller : PowershellCompletionInstallerBase
{
    public PwshCompletionInstaller(ICommandOutputProvider commandOutputProvider)
        : this(commandOutputProvider, new OctopusFileSystem(), new[] { AssemblyHelper.GetExecutablePath() })
    {
    }

    public PwshCompletionInstaller(ICommandOutputProvider commandOutputProvider, string[] executablePaths)
        : this(commandOutputProvider, new OctopusFileSystem(), executablePaths)
    {
    }

    public PwshCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executablePaths)
        : base(commandOutputProvider, fileSystem, executablePaths)
    {
    }

    public override SupportedShell SupportedShell => SupportedShell.Pwsh;
    string LinuxPwshConfigLocation => Path.Combine(HomeLocation, ".config", "powershell");

    static string WindowsPwshConfigLocation => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Powershell"
    );

    public override string ProfileLocation => ExecutionEnvironment.IsRunningOnWindows
        ? Path.Combine(WindowsPwshConfigLocation, PowershellProfileFilename)
        : Path.Combine(LinuxPwshConfigLocation, PowershellProfileFilename);

    public override string ProfileScript => base.ProfileScript.NormalizeNewLines();
}