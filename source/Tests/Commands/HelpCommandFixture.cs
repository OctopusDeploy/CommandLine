using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Octopus.CommandLine;
using Octopus.CommandLine.Commands;
using Serilog;
using Tests.Helpers;

namespace Tests.Commands
{
    [TestFixture]
    public class HelpCommandFixture
    {
        HelpCommand helpCommand;
        ICommandLocator commandLocator;
        StringWriter output;
        TextWriter originalOutput;
        ICommandOutputProvider commandOutputProvider;
        ILogger logger;

        [SetUp]
        public void SetUp()
        {
            originalOutput = Console.Out;
            output = new StringWriter();
            Console.SetOut(output);

            commandLocator = Substitute.For<ICommandLocator>();
            logger = new LoggerConfiguration().WriteTo.TextWriter(output).CreateLogger();
            commandOutputProvider = new TestCommandOutputProvider(logger);
            helpCommand = new HelpCommand(new Lazy<ICommandLocator>(() => commandLocator), commandOutputProvider);
        }

        [Test]
        public void ShouldPrintGeneralHelpWhenNoArgsGiven()
        {
            commandLocator.List()
                .Returns(new ICommandMetadata[]
                {
                    new Metadata { Name = "create-foo" },
                    new Metadata { Name = "create-bar" }
                });

            helpCommand.Execute();

            output.ToString()
                .Should()
                .MatchRegex(@"Usage: (octo|testhost|ReSharperTestRunner64) <command> \[<options>\]")
                .And.Contain("Where <command> is one of:")
                .And.Contain("create-foo");
        }

        [Test]
        public void ShouldPrintHelpForExistingCommand()
        {
            var speak = new SpeakCommand(commandOutputProvider);
            commandLocator.Find("speak").Returns(speak);
            helpCommand.Execute("speak");

            output.ToString()
                .Should()
                .MatchRegex(@"Usage: (octo|testhost|ReSharperTestRunner64) speak \[<options>\]");
        }

        [Test]
        public void ShouldFailForUnrecognisedCommand()
        {
            commandLocator.Find("foo").Returns((ICommand)null);
            helpCommand.Execute("foo");

            Assert.That(output.ToString(), Does.Contain("Command 'foo' is not supported"));
        }

        [TearDown]
        public void TearDown()
        {
            Console.SetOut(originalOutput);
        }

        class Metadata : ICommandMetadata
        {
            public string Name { get; set; }
            public string[] Aliases { get; set; }
            public string Description { get; set; }
        }
    }
}
