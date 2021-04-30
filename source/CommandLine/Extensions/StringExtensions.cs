using System;
using System.Text.RegularExpressions;

namespace Octopus.CommandLine.Extensions
{
    internal static class StringExtensions
    {
        public static string NormalizeNewLinesForNix(this string originalString)
        {
            return Regex.Replace(originalString, @"\r\n|\n\r|\n|\r", "\n");
        }

        public static string NormalizeNewLinesForWindows(this string originalString)
        {
            return Regex.Replace(originalString, @"\r\n|\n\r|\n|\r", "\r\n");
        }

        public static string NormalizeNewLines(this string originalString)
        {
            return Regex.Replace(originalString, @"\r\n|\n\r|\n|\r", Environment.NewLine);
        }
    }
}
