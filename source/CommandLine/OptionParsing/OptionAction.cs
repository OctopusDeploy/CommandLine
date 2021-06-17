using System;

namespace Octopus.CommandLine.OptionParsing
{
    public delegate void OptionAction<in TKey, in TValue>(TKey key, TValue value);
}
