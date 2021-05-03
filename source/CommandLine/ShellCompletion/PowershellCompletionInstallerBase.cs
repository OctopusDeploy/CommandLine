using System;
using System.Linq;
using System.Text;
using Octopus.CommandLine.Plumbing;

namespace Octopus.CommandLine.ShellCompletion
{
    public abstract class PowershellCompletionInstallerBase : ShellCompletionInstaller
    {
        readonly string[] executableNames;
        protected static string PowershellProfileFilename => "Microsoft.PowerShell_profile.ps1";

        public override string ProfileScript
        {
            get
            {
                var results = new StringBuilder();

                foreach (var command in executableNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    results.AppendLine($"Register-ArgumentCompleter -Native -CommandName {command} -ScriptBlock {{");
                    results.AppendLine("    param($wordToComplete, $commandAst, $cursorPosition)");
                    results.AppendLine("    $parms = $commandAst.ToString().Split(' ') | select -skip 1");
                    results.AppendLine($"    ./{command} complete $parms | % {{");
                    results.AppendLine("        [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterName', $_)");
                    results.AppendLine("    }");
                    results.AppendLine("}");
                }

                return results.ToString();
            }
        }

        public PowershellCompletionInstallerBase(ICommandOutputProvider commandOutputProvider, IOctopusFileSystem fileSystem, string[] executableNames)
            : base(commandOutputProvider, fileSystem, executableNames)
        {
            this.executableNames = executableNames;
        }
    }
}
