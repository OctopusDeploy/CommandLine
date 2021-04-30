using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Octopus.CommandLine;
using Octopus.CommandLine.Commands;

namespace Tests.Commands
{
    public class SpeakCommand : CommandBase
    {
        public SpeakCommand(ICommandOutputProvider commandOutputProvider) : base(commandOutputProvider)
        {
            var options = Options.For("default");
            options.Add<string>("message=", "The message to speak", m => { });
        }

        public override Task Execute(string[] commandLineArguments)
        {
            return Task.Run(() => Assert.Fail("This should not be called"));
        }
    }
}
