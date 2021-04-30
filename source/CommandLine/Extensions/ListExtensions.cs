using System.Collections.Generic;

namespace Octopus.CommandLine.Extensions
{
    internal static class ListExtensions
    {
        public static void AddRange<TElement>(this ICollection<TElement> source, IEnumerable<TElement> itemsToAdd)
        {
            if (itemsToAdd == null || source == null)
                return;

            foreach (var item in itemsToAdd)
            {
                source.Add(item);
            }
        }
    }
}
