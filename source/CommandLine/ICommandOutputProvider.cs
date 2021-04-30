using System;
using System.IO;
using Octopus.CommandLine.OptionParsing;

namespace Octopus.CommandLine
{
    public interface ICommandOutputProvider
    {
        bool PrintMessages { get; set; }

        void PrintHeader();

        void PrintCommandHelpHeader(string executable, string commandName, string description, TextWriter textWriter);

        void PrintCommandOptions(Options options, TextWriter textWriter);

        void Debug(string template, string propertyValue);

        void Debug(string template, params object[] propertyValues);

        void Information(string template, string propertyValue);

        void Information(string template, params object[] propertyValues);

        void Json(object o);
        void Json(object o, TextWriter writer);
        void Warning(string s);
        void Warning(string template, params object[] propertyValues);
        void Error(string template, params object[] propertyValues);
        void Error(Exception ex, string messageTemplate);
    }
}
