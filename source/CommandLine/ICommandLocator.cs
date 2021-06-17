using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine
{
    public interface ICommandLocator
    {
        ICommandMetadata[] List();
        ICommand Find(string name);
        ICommand GetCommand(string[] args);
    }
}
