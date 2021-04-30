using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Octopus.CommandLine.Commands;

namespace Octopus.CommandLine.OptionParsing
{
    public class OptionSet : KeyedCollection<string, Option>
    {
        Action<string[]> leftovers;

        public OptionSet()
        {
        }


        protected override string GetKeyForItem(Option item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];
            // This should never happen, as it's invalid for Option to be
            // constructed w/o any names.
            throw new InvalidOperationException("Option has no names!");
        }

        [Obsolete("Use KeyedCollection.this[string]")]
        protected Option GetOptionForName(string option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));
            try
            {
                return base[option];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        protected override void InsertItem(int index, Option item)
        {
            base.InsertItem(index, item);
            AddImpl(item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            var p = Items[index];
            // KeyedCollection.RemoveItem() handles the 0th item
            for (var i = 1; i < p.Names.Length; ++i)
            {
                Dictionary.Remove(p.Names[i]);
            }
        }

        protected override void SetItem(int index, Option item)
        {
            base.SetItem(index, item);
            RemoveItem(index);
            AddImpl(item);
        }

        void AddImpl(Option option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));
            var added = new List<string>(option.Names.Length);
            try
            {
                // KeyedCollection.InsertItem/SetItem handle the 0th name.
                for (var i = 1; i < option.Names.Length; ++i)
                {
                    Dictionary.Add(option.Names[i], option);
                    added.Add(option.Names[i]);
                }
            }
            catch (Exception)
            {
                foreach (var name in added)
                    Dictionary.Remove(name);
                throw;
            }
        }

        private new OptionSet Add(Option option)
        {
            base.Add(option);
            return this;
        }

        sealed class ActionOption : Option
        {
            readonly Action<OptionValueCollection> action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
                : base(prototype, description, count)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));
                this.action = action;
            }

            public override Type Type => typeof(string);

            protected override void OnParseComplete(OptionContext c)
            {
                action(c.OptionValues);
            }
        }

        sealed class ActionOption<T> : Option
        {
            readonly Action<T> action;

            public ActionOption(string prototype, string description, Action<T> action, bool sensitive, bool allowsMultiple)
                : base(prototype, description, 1, sensitive, allowsMultiple)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));
                this.action = action;
            }

            public override Type Type => typeof(T);

            protected override void OnParseComplete(OptionContext c)
            {
                if (this.OptionValueType == OptionValueType.None)
                    action(default(T));
                else
                    action(Parse<T>(c.OptionValues[0], c));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="prototype">The option name.
        /// Can be used to specify aliases by separating with a | character.
        /// If the name (or alias) ends with an = character, that means it receives a value.
        /// If the name (or alias) does not end with an = character, that means its a flag.
        /// </param>
        /// <param name="description">The description to use in the help</param>
        /// <param name="action">An action to call when the value is parsed from the command line</param>
        /// <param name="sensitive">Whether the value provided should be considered secret - ie, never log it to a log file</param>
        /// <param name="allowsMultiple">Whether the option can be supplied by the user multiple times</param>
        /// <typeparam name="T">The type of the option.
        /// String provided will automatically be converted to the desired data type.
        /// Conversion errors will throw a <see cref="CommandException"/>.
        /// </typeparam>
        public void Add<T>(string prototype, string description, Action<T> action, bool sensitive = false, bool allowsMultiple = false)
        {
            Add(new ActionOption<T>(prototype, description, action, sensitive, allowsMultiple));
        }

        protected virtual OptionContext CreateOptionContext()
        {
            return new OptionContext(this);
        }

        public OptionSet WithExtras(Action<string[]> lo)
        {
            leftovers = lo;
            return this;
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            var process = true;
            var optionContext = CreateOptionContext();
            optionContext.OptionIndex = -1;
#pragma warning disable 618
            var currentOption = GetOptionForName("<>");
#pragma warning restore 618
            var unprocessed = arguments.Where(argument => ParseOption(argument, optionContext, currentOption, ref process));
            var r = unprocessed.ToList();
            optionContext.Option?.Invoke(optionContext);

            if (leftovers != null && r.Count > 0)
            {
                leftovers(r.ToArray());
            }

            return r;
        }

        private bool ParseOption(string argument, OptionContext optionContext, Option currentOption, ref bool continueProcessing)
        {
            if (++optionContext.OptionIndex >= 0 && (continueProcessing || currentOption != null))
            {
                if (continueProcessing)
                {
                    if (argument == "--")
                        return continueProcessing = false;
                    if (!Parse(argument, optionContext))
                    {
                        if (currentOption != null)
                            return Unprocessed(null, currentOption, optionContext, argument);
                        return true;
                    }
                    return false;
                }

                if (currentOption != null)
                    return Unprocessed(null, currentOption, optionContext, argument);
                return true;
            }

            return true;
        }

        static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
                return false;
            }
            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);
            return false;
        }

        readonly Regex ValueOption = new Regex(
            @"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

#pragma warning disable 649
#pragma warning restore 649

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            flag = name = sep = value = null;
            var m = ValueOption.Match(argument);
            if (!m.Success)
            {
                return false;
            }
            flag = m.Groups["flag"].Value;
            name = m.Groups["name"].Value;
            if (m.Groups["sep"].Success && m.Groups["value"].Success)
            {
                sep = m.Groups["sep"].Value;
                value = m.Groups["value"].Value;
            }
            return true;
        }

        protected virtual bool Parse(string argument, OptionContext c)
        {
            if (c.Option != null)
            {
                ParseValue(argument, c);
                return true;
            }

            string f, n, s, v;
            if (!GetOptionParts(argument, out f, out n, out s, out v))
                return false;

            var p = this.FirstOrDefault(x => x.Names.Any(y => string.Equals(y, n, StringComparison.OrdinalIgnoreCase)));
            if (p != null)
            {
                c.OptionName = f + n;
                c.Option = p;
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        c.OptionValues.Add(n);
                        c.Option.Invoke(c);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        ParseValue(v, c);
                        break;
                }
                return true;
            }
            // no match; is it a bool option?
            if (ParseBool(argument, n, c))
                return true;
            // is it a bundled option?
// ReSharper disable once PossiblyMistakenUseOfParamsMethod
            if (ParseBundledValue(f, string.Concat(n + s + v), c))
                return true;

            return false;
        }

        public bool ShouldWaitForExit { get; }

        void ParseValue(string option, OptionContext c)
        {
            if (option != null)
                foreach (var o in c.Option.ValueSeparators != null
                    ? option.Split(c.Option.ValueSeparators, StringSplitOptions.None)
                    : new[] {option})
                {
                    c.OptionValues.Add(o);
                }
            if (c.OptionValues.Count == c.Option.MaxValueCount ||
                c.Option.OptionValueType == OptionValueType.Optional)
                c.Option.Invoke(c);
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
            {
                throw new OptionException($"Error: Found {c.OptionValues.Count} option values when expecting {c.Option.MaxValueCount}.", c.OptionName);
            }
        }

        bool ParseBool(string option, string n, OptionContext c)
        {
            string rn;
            if (n.Length >= 1 && (n[n.Length - 1] == '+' || n[n.Length - 1] == '-') &&
                Contains((rn = n.Substring(0, n.Length - 1))))
            {
                var p = this[rn];
                var v = n[n.Length - 1] == '+' ? option : null;
                c.OptionName = option;
                c.Option = p;
                c.OptionValues.Add(v);
                p.Invoke(c);
                return true;
            }
            return false;
        }

        bool ParseBundledValue(string f, string n, OptionContext c)
        {
            if (f != "-")
                return false;
            for (var i = 0; i < n.Length; ++i)
            {
                var opt = f + n[i];
                var rn = n[i].ToString();
                if (!Contains(rn))
                {
                    if (i == 0)
                        return false;
                    throw new OptionException($"Cannot bundle unregistered option '{opt}'.", opt);
                }
                var p = this[rn];
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        Invoke(c, opt, n, p);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                    {
                        var v = n.Substring(i + 1);
                        c.Option = p;
                        c.OptionName = opt;
                        ParseValue(v.Length != 0 ? v : null, c);
                        return true;
                    }
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
                }
            }
            return true;
        }

        static void Invoke(OptionContext c, string name, string value, Option option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        const int OptionWidth = 29;

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (var p in this)
            {
                var written = 0;
                if (!WriteOptionPrototype(o, p, ref written))
                    continue;

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                var lines = GetLines(GetDescription(p.Description));
                o.WriteLine(lines[0]);
                var prefix = new string(' ', OptionWidth);
                for (var i = 1; i < lines.Count; ++i)
                {
                    o.Write(prefix);
                    o.WriteLine(lines[i]);
                }
            }
        }

        bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
        {
            var names = p.Names;

            var i = GetNextOptionIndex(names, 0);
            if (i == names.Length)
                return false;

            if (names[i].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }

            for (i = GetNextOptionIndex(names, i + 1);
                i < names.Length;
                i = GetNextOptionIndex(names, i + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, names[i].Length == 1 ? "-" : "--");
                Write(o, ref written, names[i]);
            }

            if (p.OptionValueType == OptionValueType.Optional ||
                p.OptionValueType == OptionValueType.Required)
            {
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, "[");
                }
                Write(o, ref written, "=" + GetArgumentName(0, p.MaxValueCount, p.Description));
                var sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
                    ? p.ValueSeparators[0]
                    : " ";
                for (var c = 1; c < p.MaxValueCount; ++c)
                {
                    Write(o, ref written, sep + GetArgumentName(c, p.MaxValueCount, p.Description));
                }
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, "]");
                }
            }
            return true;
        }

        static int GetNextOptionIndex(string[] names, int i)
        {
            while (i < names.Length && names[i] == "<>")
            {
                ++i;
            }
            return i;
        }

        static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        static string GetArgumentName(int index, int maxIndex, string description)
        {
            if (description == null)
                return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
            string[] nameStart;
            if (maxIndex == 1)
                nameStart = new[] {"{0:", "{"};
            else
                nameStart = new[] {"{" + index + ":"};
            for (var i = 0; i < nameStart.Length; ++i)
            {
                int start, j = 0;
                do
                {
                    start = description.IndexOf(nameStart[i], j, StringComparison.Ordinal);
                } while (start >= 0 && j != 0 && description[j++ - 1] == '{');
                if (start == -1)
                    continue;
                var end = description.IndexOf("}", start, StringComparison.Ordinal);
                if (end == -1)
                    continue;
                return description.Substring(start + nameStart[i].Length, end - start - nameStart[i].Length);
            }
            return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
        }

        static string GetDescription(string description)
        {
            if (description == null)
                return string.Empty;
            var sb = new StringBuilder(description.Length);
            var start = -1;
            for (var i = 0; i < description.Length; ++i)
            {
                switch (description[i])
                {
                    case '{':
                        if (i == start)
                        {
                            sb.Append('{');
                            start = -1;
                        }
                        else if (start < 0)
                            start = i + 1;
                        break;
                    case '}':
                        if (start < 0)
                        {
                            if ((i + 1) == description.Length || description[i + 1] != '}')
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++i;
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(description.Substring(start, i - start));
                            start = -1;
                        }
                        break;
                    case ':':
                        if (start < 0)
                            goto default;
                        start = i + 1;
                        break;
                    default:
                        if (start < 0)
                            sb.Append(description[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        static List<string> GetLines(string description)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(description))
            {
                lines.Add(string.Empty);
                return lines;
            }
            var length = 80 - OptionWidth - 2;
            int start = 0, end;
            do
            {
                end = GetLineEnd(start, length, description);
                var cont = false;
                if (end < description.Length)
                {
                    var c = description[end];
                    if (c == '-' || (char.IsWhiteSpace(c) && c != '\n'))
                        ++end;
                    else if (c != '\n')
                    {
                        cont = true;
                        --end;
                    }
                }
                lines.Add(description.Substring(start, end - start));
                if (cont)
                {
                    lines[lines.Count - 1] += "-";
                }
                start = end;
                if (start < description.Length && description[start] == '\n')
                    ++start;
            } while (end < description.Length);
            return lines;
        }

        static int GetLineEnd(int start, int length, string description)
        {
            var end = Math.Min(start + length, description.Length);
            var sep = -1;
            for (var i = start; i < end; ++i)
            {
                switch (description[i])
                {
                    case ' ':
                    case '\t':
                    case '\v':
                    case '-':
                    case ',':
                    case '.':
                    case ';':
                        sep = i;
                        break;
                    case '\n':
                        return i;
                }
            }
            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }
    }
}
