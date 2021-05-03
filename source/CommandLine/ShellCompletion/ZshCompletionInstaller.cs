
using System.Linq;
using System.Text;
using Octopus.CommandLine.Commands;
using Octopus.CommandLine.Extensions;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public class ZshCompletionInstaller : ShellCompletionInstaller
    {
        readonly string[] executableNames;

        public override SupportedShell SupportedShell => SupportedShell.Zsh;
        public override string ProfileLocation => $"{HomeLocation}/.zshrc";
        public override string ProfileScript
        {
            get
            {
                var sanitisedAppName = executableNames.First().ToLower().Replace(".", "_").Replace(" ", "_");
                var functionName = $"_{sanitisedAppName}_zsh_complete";
                var result = new StringBuilder();
                result.AppendLine(functionName + "()");
                result.AppendLine("{");
                result.AppendLine($@"    local completions=(""$({executableNames.First()} complete $words)"")");
                result.AppendLine(@"    reply=( ""${(ps:\n:)completions}"" )");
                result.AppendLine("}");
                foreach (var executable in executableNames)
                    result.AppendLine($"compctl -K {functionName} {executable}");

                return result.ToString().NormalizeNewLinesForNix();
            }
        }

        public ZshCompletionInstaller(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executableNames)
            : base(commandOutputProvider, fileSystem)
        {
            this.executableNames = executableNames;
        }
    }
}
