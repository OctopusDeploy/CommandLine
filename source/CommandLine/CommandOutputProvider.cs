﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Octopus.CommandLine.OptionParsing;
using Serilog;
using Serilog.Events;

namespace Octopus.CommandLine
{
    public class CommandOutputProvider : ICommandOutputProvider
    {
        readonly ILogger logger;

        readonly string applicationName;
        readonly ICommandOutputJsonSerializer commandOutputJsonSerializer;

        public CommandOutputProvider(string applicationName, ILogger logger)
            : this(applicationName, new DefaultCommandOutputJsonSerializer(), logger)
        {
        }

        public CommandOutputProvider(string applicationName, ICommandOutputJsonSerializer commandOutputJsonSerializer, ILogger logger)
        {
            this.applicationName = applicationName;
            this.commandOutputJsonSerializer = commandOutputJsonSerializer;
            this.logger = logger;
            PrintMessages = true; // unless told otherwise
        }

        public bool PrintMessages { get; set; }

        public void PrintHeader()
        {
            if (PrintMessages)
            {
                logger.Information($"{GetAppName()}, version {GetAppVersion()}");
                logger.Information(string.Empty);
            }
        }

        public string GetAppName() => applicationName;

        public string GetAppVersion()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
                throw new ApplicationException("Unable to determine entry assembly");
            var assemblyInformationalVersionAttribute = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (assemblyInformationalVersionAttribute != null)
                return assemblyInformationalVersionAttribute.InformationalVersion;
            return entryAssembly.GetName().Version.ToString();
        }

        public void PrintCommandHelpHeader(string executable, string commandName, string description, TextWriter textWriter)
        {
            if (PrintMessages)
            {
                Console.ResetColor();
                textWriter.WriteLine(description);
                textWriter.WriteLine();
                textWriter.Write("Usage: ");
                Console.ForegroundColor = ConsoleColor.White;
                textWriter.WriteLine($"{executable} {commandName} [<options>]");
                Console.ResetColor();
                textWriter.WriteLine();
                textWriter.WriteLine("Where [<options>] is any of: ");
                textWriter.WriteLine();
            }
        }

        public void PrintCommandOptions(Options options, TextWriter writer)
        {
            if (PrintMessages)
                foreach (var g in options.OptionSets.Keys.Reverse())
                {
                    writer.WriteLine($"{g}: ");
                    writer.WriteLine();
                    options.OptionSets[g].WriteOptionDescriptions(writer);
                    writer.WriteLine();
                }
        }

        public void Debug(string template, string propertyValue)
        {
            if (PrintMessages)
                logger.Debug(template, propertyValue);
        }

        public void Debug(string template, params object[] propertyValues)
        {
            if (PrintMessages)
                logger.Debug(template, propertyValues);
        }

        public void Information(string template, string propertyValue)
        {
            if (PrintMessages)
                logger.Information(template, propertyValue);
        }

        public void Information(string template, params object[] propertyValues)
        {
            if (PrintMessages)
                logger.Information(template, propertyValues);
        }

        public void Json(object o)
        {
            logger.Information(commandOutputJsonSerializer.SerializeObjectToJson(o));
        }

        public void Json(object o, TextWriter writer)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                Converters = new JsonConverterCollection { new StringEnumConverter() }
            };
            writer.WriteLine(JsonConvert.SerializeObject(o, settings));
        }

        public void Warning(string s)
        {
            if (PrintMessages)
                logger.Warning(s);
        }

        public void Warning(string template, params object[] propertyValues)
        {
            if (PrintMessages)
                logger.Warning(template, propertyValues);
        }

        public void Error(string template, params object[] propertyValues)
        {
            if (PrintMessages)
                logger.Error(template, propertyValues);
        }

        public void Write(LogEventLevel logEventLevel, string messageTemplate, params object[] propertyValues)
        {
            if (PrintMessages)
                logger.Write(logEventLevel, messageTemplate, propertyValues);
        }

        public void Error(Exception ex, string messageTemplate)
        {
            if (PrintMessages)
                logger.Error(ex, messageTemplate);
        }

    }
}
