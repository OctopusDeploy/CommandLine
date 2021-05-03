using System.IO;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public class PowershellCompletionInstaller : PowershellCompletionInstallerBase
    {
        public override SupportedShell SupportedShell => SupportedShell.Powershell;
        private static string WindowsPowershellConfigLocation => Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
            "WindowsPowershell"
        );
        public override string ProfileLocation => Path.Combine(WindowsPowershellConfigLocation, PowershellProfileFilename);
        public override string ProfileScript => base.ProfileScript.NormalizeNewLinesForWindows();

        public PowershellCompletionInstaller(ICommandOutputProvider commandOutputProvider, string[] executableNames)
            : this(commandOutputProvider, new OctopusFileSystem(), executableNames) { }

        public PowershellCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executableNames)
            : base(commandOutputProvider, fileSystem, executableNames) { }

        public override void Install(bool dryRun)
        {
            if (ExecutionEnvironment.IsRunningOnNix || ExecutionEnvironment.IsRunningOnMac || ExecutionEnvironment.IsRunningOnMono)
                throw new CommandException("Unable to install for powershell on non-windows platforms. Please use --shell=pwsh instead.");
            base.Install(dryRun);
        }
    }
}
