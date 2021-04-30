using System;

namespace Octopus.CommandLine.OptionParsing
{
    public class OptionException : Exception
    {
        public OptionException()
        {
        }

        public OptionException(string message, string optionName)
            : base(message)
        {
            OptionName = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            OptionName = optionName;
        }

        public string OptionName { get; }
    }
}