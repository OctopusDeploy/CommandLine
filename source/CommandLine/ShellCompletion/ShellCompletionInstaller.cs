using System.IO;
using System.Linq;
using System.Text;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public abstract class ShellCompletionInstaller : IShellCompletionInstaller
    {
        private readonly ICommandOutputProvider commandOutputProvider;
        private readonly IOctopusFileSystem fileSystem;
        readonly string[] executableNames;
        public static string HomeLocation => System.Environment.GetEnvironmentVariable("HOME");
        public abstract string ProfileLocation { get; }
        public abstract string ProfileScript { get; }
        public abstract SupportedShell SupportedShell { get; }

        public ShellCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executableNames)
        {
            this.commandOutputProvider = commandOutputProvider;
            this.fileSystem = fileSystem;
            this.executableNames = executableNames;
        }

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

        public string AllShellsPrefix => $"# start: Octopus Command Line App ({executableNames.First()}) Autocomplete script";
        public string AllShellsSuffix => $"# end: Octopus Command Line App ({executableNames.First()}) Autocomplete script";
    }
}
