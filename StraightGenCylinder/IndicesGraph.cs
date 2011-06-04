﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    /// <summary>
    /// A static class for operations on graph where each vertes is a zero-based index.
    /// </summary>
    static class IndicesGraph
    {
        /// <summary>
        /// Converts a graph represented by a collection of edges to neighborhood-lists representation.
        /// </summary>
        /// <param name="edges">Graph edges. Each entry (i,j) represents edge from node i to node j.</param>
        /// <returns>A neighborhood-list representation of the graph. <c>result[i]</c> is a collection of neighbors of node i.</returns>
        [Pure]
        public static IList<IEnumerable<int>> ToNeighborhoodLists(this IEnumerable<Tuple<int, int>> edges)
        {
            Contract.Requires(edges != null); 
            Contract.Requires(Contract.ForAll(edges, edge => edge.Item1 >= 0 && edge.Item2 >= 0));
            Contract.Ensures(Contract.Result<IList<IEnumerable<int>>>() != null);

            var allNodes = Enumerable.Range(0, MaxNodeIndex(edges) + 1);
            var nodeToNeighbors = allNodes.ToDictionary(
                x => x, 
                _ => new List<int>());

            foreach (var pair in edges)
                nodeToNeighbors[pair.Item1].Add(pair.Item2);

            var result = allNodes.Select(x => nodeToNeighbors[x]).ToArray();
            return result;
        }

        /// <summary>
        /// Computes the maximal node index in a graph given by a list of edges.
        /// </summary>
        /// <param name="edges">The list of edges. The tuple (i,j) is an edge from node i to node j.</param>
        /// <returns>The maximal node index in <paramref name="edges"/>.</returns>
        [Pure]
        public static int MaxNodeIndex(this IEnumerable<Tuple<int, int>> edges)
        {
            Contract.Requires(edges != null); // edges is a valid enumeration
            Contract.Requires(edges.Any());   // we have at-least one edge
            Contract.Requires(Contract.ForAll(edges, edge => edge.Item1 >= 0 && edge.Item2 >= 0)); // edges are valid indices
            Contract.Ensures(Contract.Result<int>() >= 0); // result is a valid index
            Contract.Ensures(Contract.ForAll(edges, edge => edge.Item1 <= Contract.Result<int>() && edge.Item2 <= Contract.Result<int>())); // result is actually the max edge

            return edges.SelectMany(x => x.Enumerate()).Max();
        }
    }
}