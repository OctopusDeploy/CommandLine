using System;

namespace Octopus.CommandLine;

public interface ICommandMetadata
{
    string Name { get; }
    string[] Aliases { get; }
    string Description { get; }
}