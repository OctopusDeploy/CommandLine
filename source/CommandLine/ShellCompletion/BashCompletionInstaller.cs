using System.Linq;
using System.Text;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public class BashCompletionInstaller : ShellCompletionInstaller
    {
        readonly string[] executableNames;
        public override SupportedShell SupportedShell => SupportedShell.Bash;
        public override string ProfileLocation => $"{HomeLocation}/.bashrc";
        public override string ProfileScript
        {
            get
            {
                var sanitisedAppName = executableNames.First().ToLower().Replace(".", "_").Replace(" ", "_");
                var functionName = $"_{sanitisedAppName}_bash_complete";
                var result = new StringBuilder();
                result.AppendLine(functionName + "()");
                result.AppendLine("{");
                result.AppendLine("    local params=${COMP_WORDS[@]:1}");
                result.AppendLine($@"    local completions=""$({executableNames.First()} complete ${{params}})");
                result.AppendLine(@"    COMPREPLY=( $(compgen -W ""$completions"") )");
                result.AppendLine("}");

                foreach (var executable in executableNames)
                    result.AppendLine($"complete -F {functionName} {executable}");

                return result.ToString().NormalizeNewLinesForNix();
            }
        }

        public BashCompletionInstaller(ICommandOutputProvider commandOutputProvider, string[] executableNames)
            : this(commandOutputProvider, new OctopusFileSystem(), executableNames) { }

        public BashCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executableNames)
            : base(commandOutputProvider, fileSystem, executableNames)
        {
            this.executableNames = executableNames;
        }
    }
}
