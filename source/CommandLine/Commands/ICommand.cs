using System;
using System.IO;
using System.Threading.Tasks;

namespace Octopus.CommandLine.Commands;

public interface ICommand
{
    ICommandMetadata CommandMetadata { get; }
    void GetHelp(TextWriter writer, string[] args);
    Task Execute(string[] commandLineArguments);
}