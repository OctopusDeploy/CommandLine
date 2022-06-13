using System;
using System.Diagnostics;
using System.IO;

namespace Octopus.CommandLine.Extensions;

public static class AssemblyHelper
{
    public static string GetExecutablePath()
        => Process.GetCurrentProcess()?.MainModule?.FileName ?? throw new ApplicationException("Unable to determine executable name");

    public static string GetExecutableName()
        => Path.GetFileNameWithoutExtension(GetExecutablePath());
}