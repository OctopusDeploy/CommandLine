using System;
using System.IO;
using System.Linq;
using System.Text;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public enum SupportedShell
    {
        Unspecified,
        Pwsh,
        Zsh,
        Bash,
        Powershell
    }

    public abstract class ShellCompletionInstaller : IShellCompletionInstaller
    {
        readonly ICommandOutputProvider commandOutputProvider;
        readonly IOctopusFileSystem fileSystem;
        protected readonly string[] ExecutablePaths;

        public ShellCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executablePaths)
        {
            this.commandOutputProvider = commandOutputProvider;
            this.fileSystem = fileSystem;
            //some DI containers will pass an empty array, instead of choosing a less specific ctor that doesn't require the missing param
            ExecutablePaths = executablePaths.Length == 0 ? new[] { AssemblyHelper.GetExecutablePath() } : executablePaths;
        }

        public static string HomeLocation => Environment.GetEnvironmentVariable("HOME");
        public abstract string ProfileLocation { get; }
        public abstract string ProfileScript { get; }
        public abstract SupportedShell SupportedShell { get; }

        public string AllShellsPrefix => $"# start: Octopus Command Line App ({Path.GetFileName(ExecutablePaths.First())}) Autocomplete script";
        public string AllShellsSuffix => $"# end: Octopus Command Line App ({Path.GetFileName(ExecutablePaths.First())}) Autocomplete script";

        public virtual void Install(bool dryRun)
        {
            commandOutputProvider.Information($"Installing scripts in {ProfileLocation}");
            var tempOutput = new StringBuilder();
            if (fileSystem.FileExists(ProfileLocation))
            {
                var profileText = fileSystem.ReadAllText(ProfileLocation);
                if (!dryRun)
                {
                    if (profileText.Contains(AllShellsPrefix) || profileText.Contains(AllShellsSuffix) || profileText.Contains(ProfileScript))
                    {
                        if (!profileText.Contains(ProfileScript)){
                            var message =
                                $"Looks like command line completion is already installed, but points to a different executable.{Environment.NewLine}" +
                                $"Please manually edit the file {ProfileLocation} to remove the existing auto complete script and then re-install.";
                            throw new CommandException(message);
                        }

                        commandOutputProvider.Information("Looks like command line completion is already installed. Nothing to do.");
                        return;
                    }

                    var backupPath = ProfileLocation + ".orig";
                    commandOutputProvider.Information($"Backing up the existing profile to {backupPath}");
                    fileSystem.CopyFile(ProfileLocation, backupPath);
                }

                commandOutputProvider.Information($"Updating profile at {ProfileLocation}");
                tempOutput.AppendLine(profileText);
            }
            else
            {
                commandOutputProvider.Information($"Creating profile at {ProfileLocation}");
                fileSystem.EnsureDirectoryExists(Path.GetDirectoryName(ProfileLocation));
            }

            tempOutput.AppendLine(AllShellsPrefix);
            tempOutput.AppendLine(ProfileScript);
            tempOutput.AppendLine(AllShellsSuffix);

            if (dryRun)
            {
                commandOutputProvider.Warning("Preview of script changes: ");
                commandOutputProvider.Information(tempOutput.ToString());
                commandOutputProvider.Warning("Preview of script changes finished. ");
            }
            else
            {
                fileSystem.OverwriteFile(ProfileLocation, tempOutput.ToString());
                commandOutputProvider.Warning("All Done! Please reload your shell or dot source your profile to get started! Use the <tab> key to autocomplete.");
            }
        }
    }
}
