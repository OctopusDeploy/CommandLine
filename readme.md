## Octopus.CommandLine

This repository contains Octopus.CommandLine, the command line parsing library used by many of Octopus apps.

Please see [Contributing](CONTRIBUTING.md).

## Usage

### Setup references

Add a reference to Octopus.CommandLine, Newtonsoft.Json and Autofac:
```bash
dotnet add package Octopus.CommandLine
dotnet add package Autofac
```
Note:
* Autofac can be swapped for your container of choice

### Build your container
```c#
private static IContainer BuildContainer()
{
    var builder = new ContainerBuilder();

    //configure logging - you're already likely doing this
    Log.Logger = new LoggerConfiguration()
        .WriteTo.ColoredConsole(outputTemplate: "{Message}{NewLine}{Exception}")
        .CreateLogger();
    builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

    //Sometimes you'll want to provide your own implementation of this, but for most usages
    //the default one works fine
    builder.RegisterType<DefaultCommandOutputProvider>()
        .WithParameter("applicationName", "My sample application")
        .As<ICommandOutputProvider>()
        .SingleInstance();
        
    //Register all the built-in shell completion installers
    builder.RegisterAssemblyTypes(typeof(ICommand).Assembly)
        .Where(t => t.IsAssignableTo<IShellCompletionInstaller>())
        .AsImplementedInterfaces()
        .AsSelf();
        
    //register the command locator and other in-built commands
    builder.RegisterType<CommandLocator>().As<ICommandLocator>();
    builder.RegisterAssemblyTypes(typeof(ICommand).Assembly).As<ICommand>().AsSelf();

    //Register any implementations in your app
    var thisAssembly = typeof(Program).GetTypeInfo().Assembly;
    builder.RegisterAssemblyTypes(thisAssembly).As<ICommand>().AsSelf();

    return builder.Build();
}
```

### Find and execute the command in your `Main` method

```c#
static async Task<int> Main(string[] args)
{
    var container = BuildContainer();
    var commandLocator = container.Resolve<ICommandLocator>();
    try
    {
        var command = commandLocator.GetCommand(args);
        await command.Execute(args.Skip(1).ToArray());
        return 0;
    }
    catch (CommandException ex)
    {
        //this is a "known error" - ie, one we dont want a stack trace for
        Log.Error(ex.Message);
        return 1;
    }
    catch (Exception ex)
    {
        //this is an unknown error - log with stack trace
        Log.Error(ex, ex.Message);
        return 2;
    }
}
```

### Create your first command

```c#
[Command("mycommand", Description = "Does the thing")]
public class MyCommand : CommandBase
{
    private readonly ICommandOutputProvider _commandOutputProvider;
    private bool dryRun;

    public MyCommand(ICommandOutputProvider commandOutputProvider) : base(commandOutputProvider)
    {
        _commandOutputProvider = commandOutputProvider;
        var options = Options.For("My Command");
        options.Add<bool>("dryRun",
            "Dry run will output the proposed changes to console, instead of writing to disk.",
            v => dryRun = true);
    }

    public override Task Execute(string[] commandLineArguments)
    {
        var remainingArguments = Options.Parse(commandLineArguments);
        if (remainingArguments.Count > 0)
            throw new CommandException("Unrecognized command arguments: " + string.Join(", ", remainingArguments));

        _commandOutputProvider.Information("This is my command");
        _commandOutputProvider.Information(dryRun
            ? "This is a dry-run; skipping doing the thing"
            : "Doing the thing");
        return Task.CompletedTask;
    }
```

## Test it out:
```powershell
PS> ./myapp.exe help
```
