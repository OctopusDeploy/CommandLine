using System;
using System.Diagnostics;
using System.IO;

namespace Octopus.CommandLine.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess()?.MainModule?.FileName ?? throw new ApplicationException("Unable to determine executable name");
        }

        public static string GetExecutableName()
        {
            return Path.GetFileNameWithoutExtension(GetExecutablePath());
        }
    }
}
