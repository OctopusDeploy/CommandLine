
using System.IO;
using System.Linq;
using System.Text;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public class ZshCompletionInstaller : ShellCompletionInstaller
    {
        readonly string[] executablePaths;

        public override SupportedShell SupportedShell => SupportedShell.Zsh;
        public override string ProfileLocation => $"{HomeLocation}/.zshrc";
        public override string ProfileScript
        {
            get
            {
                var sanitisedAppName = Path.GetFileName(executablePaths.First()).ToLower().Replace(".", "_").Replace(" ", "_");
                var functionName = $"_{sanitisedAppName}_zsh_complete";
                var result = new StringBuilder();
                result.AppendLine(functionName + "()");
                result.AppendLine("{");
                result.AppendLine($@"    local completions=(""$({executablePaths.First()} complete $words)"")");
                result.AppendLine(@"    reply=( ""${(ps:\n:)completions}"" )");
                result.AppendLine("}");
                foreach (var executable in executablePaths)
                    result.AppendLine($"compctl -K {functionName} {executable}");

                return result.ToString().NormalizeNewLinesForNix();
            }
        }

        public ZshCompletionInstaller(ICommandOutputProvider commandOutputProvider)
            : this(commandOutputProvider, new OctopusFileSystem(), new[] { AssemblyExtensions.GetExecutablePath() }) { }

        public ZshCompletionInstaller(ICommandOutputProvider commandOutputProvider, string[] executablePaths)
            : this(commandOutputProvider, new OctopusFileSystem(), executablePaths) { }

        public ZshCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executablePaths)
            : base(commandOutputProvider, fileSystem, executablePaths)
        {
            this.executablePaths = executablePaths;
        }
    }
}
