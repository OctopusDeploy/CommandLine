//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using Octopus.CommandLine.Extensions;

namespace Octopus.CommandLine.OptionParsing
{
    public class Options
    {
        public Options()
        {
            OptionSets = new Dictionary<string, OptionSet>();
        }

        public OptionSet For(string groupName)
        {
            if (!OptionSets.ContainsKey(groupName))
            {
                var o = new OptionSet();
                OptionSets[groupName] = o;
                return o;
            }

            return OptionSets[groupName];
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            var combined = new OptionSet();
            foreach (var group in OptionSets.Keys)
                combined.AddRange(OptionSets[group]);

            return combined.Parse(arguments);
        }

        public Dictionary<string, OptionSet> OptionSets { get; private set; }
    }
}
