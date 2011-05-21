using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StraightGenCylinder
{
    static class Graph
    {
        public static IList<IEnumerable<int>> ToNeighborhoodLists(this IEnumerable<Tuple<int, int>> edges)
        {
            var allNodes = Enumerable.Range(0, MaxNode(edges) + 1);
            var nodeToNeighbors = allNodes.ToDictionary(
                x => x, 
                _ => new List<int>());

            foreach (var pair in edges)
                nodeToNeighbors[pair.Item1].Add(pair.Item2);

            var result = allNodes.Select(x => nodeToNeighbors[x]).ToArray();
            return result;
        }

        public static int MaxNode(this IEnumerable<Tuple<int, int>> edges)
        {
            return edges.SelectMany(x => x.Enumerate()).Max();
        }
    }
}
