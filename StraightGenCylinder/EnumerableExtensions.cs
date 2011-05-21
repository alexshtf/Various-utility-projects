using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    static class EnumerableExtensions
    {
        public static IEnumerable<Tuple<T, T>> SeqPairs<T>(this IEnumerable<T> items)
        {
            Contract.Requires(items != null);
            Contract.Ensures(Contract.Result<IEnumerable<Tuple<T, T>>>() != null);

            return items.Zip(items.Skip(1), (t, s) => Tuple.Create(t, s));
        }

        /// <summary>
        /// Returns an item from the source enumeration that minimizes a certain value.
        /// </summary>
        /// <typeparam name="T">The type of items in the source enumeration</typeparam>
        /// <typeparam name="S">The item of values to minimize</typeparam>
        /// <param name="source">The enumeration of items to minimize over</param>
        /// <param name="itemValue">The function to calculate item value for each item.</param>
        /// <returns><c>x</c> from <paramref name="source"/> that minimizes itemValue(x)</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is an empty enumeration.</exception>
        public static T Minimizer<T, S>(this IEnumerable<T> source, Converter<T, S> itemValue)
            where S : IComparable<S>
        {
            Contract.Requires(source != null);
            Contract.Requires(itemValue != null);
            Contract.Requires(source.Any() == true);

            Contract.Ensures(source.Contains(Contract.Result<T>())); // the source collection contains the minimizer
            Contract.Ensures(Contract.ForAll(source, item =>
                Comparer<S>.Default.Compare(itemValue(item), itemValue(Contract.Result<T>())) >= 0)); // all items are greater or equal to the minimizer

            var itemsWithKeys = source.Select(x => new { Item = x, Key = itemValue(x) });
            var minKey = itemsWithKeys.Min(x => x.Key);             // x.Item2 is itemValue(x). minValue will be the minimum

            // now we take the first item we find that has the minimum value.
            var minimizer = itemsWithKeys.First(pair => pair.Key.CompareTo(minKey) == 0).Item;

            return minimizer;
        }
    }
}
