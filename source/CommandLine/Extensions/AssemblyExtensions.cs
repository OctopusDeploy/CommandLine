using System.Reflection;

namespace Octopus.CommandLine.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetExecutableName()
        {
            return Assembly.GetEntryAssembly()?.GetName().Name ?? "octo";
        }
    }
}
