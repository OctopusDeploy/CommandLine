﻿namespace Octopus.CommandLine
{
    public delegate void OptionAction<in TKey, in TValue>(TKey key, TValue value);
}
