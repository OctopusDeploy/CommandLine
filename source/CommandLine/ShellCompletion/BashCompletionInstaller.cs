using System.IO;
using System.Linq;
using System.Text;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public class BashCompletionInstaller : ShellCompletionInstaller
    {
        readonly string[] executablePaths;
        public override SupportedShell SupportedShell => SupportedShell.Bash;
        public override string ProfileLocation => $"{HomeLocation}/.bashrc";
        public override string ProfileScript
        {
            get
            {
                var sanitisedAppName = Path.GetFileName(executablePaths.First()).ToLower().Replace(".", "_").Replace(" ", "_");
                var functionName = $"_{sanitisedAppName}_bash_complete";
                var result = new StringBuilder();
                result.AppendLine($"{functionName}()");
                result.AppendLine("{");
                result.AppendLine("    local params=${COMP_WORDS[@]:1}");
                result.AppendLine($@"    local completions=""$({executablePaths.First()} complete ${{params}})");
                result.AppendLine(@"    COMPREPLY=( $(compgen -W ""$completions"") )");
                result.AppendLine("}");

                foreach (var executable in executablePaths)
                    result.AppendLine($"complete -F {functionName} {executable}");

                return result.ToString().NormalizeNewLinesForNix();
            }
        }

        public BashCompletionInstaller(ICommandOutputProvider commandOutputProvider, string[] executablePaths)
            : this(commandOutputProvider, new OctopusFileSystem(), executablePaths) { }

        public BashCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executablePaths)
            : base(commandOutputProvider, fileSystem, executablePaths)
        {
            this.executablePaths = executablePaths;
        }
    }
}
