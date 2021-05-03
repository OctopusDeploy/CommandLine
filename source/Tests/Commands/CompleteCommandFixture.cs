using System;
using System.IO;
using System.Threading.Tasks;
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
    public class CompleteCommandFixture
    {
        CompleteCommand completeCommand;
        ICommandOutputProvider commandOutputProvider;
        ILogger logger;

        TextWriter originalOutput;
        StringWriter output;
        ICommandLocator commandLocator;

        [SetUp]
        public void SetUp()
        {
            originalOutput = Console.Out;
            output = new StringWriter();
            Console.SetOut(output);

            commandLocator = Substitute.For<ICommandLocator>();
            logger = new LoggerConfiguration().WriteTo.TextWriter(output).CreateLogger();
            commandOutputProvider = new TestCommandOutputProvider(logger);
            commandLocator.List()
                .Returns(new ICommandMetadata[]
                {
                    new CommandAttribute("test"),
                    new CommandAttribute("help")
                });
            var helpCommand = new HelpCommand(new Lazy<ICommandLocator>(() => commandLocator), commandOutputProvider);
            var testCommand = new TestCommand(commandOutputProvider);
            commandLocator.Find("help").Returns(helpCommand);
            commandLocator.Find("test").Returns(testCommand);
            completeCommand = new CompleteCommand(new Lazy<ICommandLocator>(() => commandLocator), commandOutputProvider);
        }

        [Test]
        public async Task ShouldReturnSubCommandSuggestions()
        {
            await completeCommand.Execute(new[] { "he" });

            output.ToString()
                .Should()
                .Contain("help")
                .And.NotContain("test");
        }

        [Test]
        public async Task ShouldReturnParameterSuggestions()
        {
            await completeCommand.Execute(new[] { "test", "--ap" });
            output.ToString()
                .Should()
                .Contain("--apiKey");
        }

        [Test]
        public async Task ShouldReturnCommonOptionsWhenSingleEmptyParameter()
        {
            await completeCommand.Execute(new[] { "--" });
            output.ToString()
                .Should()
                .Contain("--helpOutputFormat");
        }

        [Test]
        public async Task ShouldReturnOptionSuggestions()
        {
            await completeCommand.Execute(new[] { "--helpOut" });
            output.ToString()
                .Should()
                .Contain("--helpOutputFormat")
                .And.NotContain("--help\n");
        }

        [Test]
        public async Task ShouldReturnAllSubCommandsWhenEmptyArguments()
        {
            await completeCommand.Execute(new[] { "" });
            output.ToString()
                .Should()
                .Contain("help")
                .And.Contain("test");
        }

        [Test]
        public async Task ShouldStopSubCommandCompletionAfterOptionSuggestion()
        {
            await completeCommand.Execute(new[] { "test", "--api", "API-KEY", "--u" });
            output.ToString()
                .Should()
                .Contain("--url");
        }

        [Test]
        [TestCase("--help")]
        [TestCase("foo --help")]
        public async Task SupportsHelpOption(string commandLine)
        {
            await completeCommand.Execute(commandLine.Split(' '));
            output.ToString()
                .Should()
                .Contain("Where <command> is the current command line to filter auto-completions");
        }

        [TearDown]
        public void TearDown()
        {
            Console.SetOut(originalOutput);
        }
    }

    [Command("test", Description = "test command")]
    public class TestCommand : CommandBase
    {
        public TestCommand(ICommandOutputProvider commandOutputProvider) : base(commandOutputProvider)
        {
            var options = Options.For("Test group");
            options.Add<string>("apiKey", "api key", v => ApiKey = v);
            options.Add<string>("url", "url", v => Url = v);
        }

        public string Url { get; set; }

        public string ApiKey { get; set; }

        public override Task Execute(string[] commandLineArguments)
        {
            return Task.Run(() => 0);
        }
    }
}
