using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    static class Dijkstra
    {
        public static IList<int> ShortestPath(IEnumerable<Tuple<int, int>> edges, Func<int, int, double> weight, int source, int target)
        {
            Contract.Requires(edges != null);
            Contract.Requires(weight != null);
            Contract.Requires(source <= edges.MaxNodeIndex());
            Contract.Requires(target <= edges.MaxNodeIndex());

            var previous = ShortestPaths(edges, weight, source);

            var path = new List<int>();
            var current = target;
            while (current != -1)
            {
                path.Add(current);
                current = previous[current];
            }
            path.Reverse();

            if (path[0] == source)
                return path;
            else
                return new int[0]; // empty path.. source and target are not connected
        }

        private static int[] ShortestPaths(IEnumerable<Tuple<int, int>> edges, Func<int, int, double> weight, int source)
        {
            var neighborsOf = edges.ToNeighborhoodLists();
            var vertices = Enumerable.Range(0, neighborsOf.Count);

            // distanceTo will store the distance from source to any other vertex. At the beginning according to Dijkstra algorithm it's infinity for all
            // vertices except the source
            var distanceTo = Enumerable.Repeat(double.PositiveInfinity, neighborsOf.Count).ToArray();
            distanceTo[source] = 0; // distance from the source to itself 

            // previous[v] will store the previous vertex in the shortest path from source to v. Initialized to -1, which means no previous vertex
            // as we don't know the shortest path yet.
            var previous = Enumerable.Repeat(-1, neighborsOf.Count).ToArray();

            var priorityQueue = new SortedSet<int>(DelegateComparer.Create<int>((x, y) => // compare vertices according to their distance from source
            {
                var weightCompare = distanceTo[x].CompareTo(distanceTo[y]);
                if (weightCompare == 0)
                    return x.CompareTo(y);
                else
                    return weightCompare;
            }));
            foreach (var v in vertices)
                priorityQueue.Add(v);

            // the main loop of the Dijkstra algorithm
            while (priorityQueue.Count > 0)
            {
                // we remove the min-distance vertex from the priority queue
                var v = priorityQueue.First();
                priorityQueue.Remove(v);

                // update all neighbors of v with new distances
                foreach (var u in neighborsOf[v])
                {
                    var newDistance = distanceTo[v] + weight(v, u);
                    if (newDistance < distanceTo[u])
                    {
                        // we have to add and remove u from the priority queue during distance update, otherwise the priority queue will incorrectly
                        // return the minimal-distance node.
                        priorityQueue.Remove(u);
                        distanceTo[u] = newDistance;
                        priorityQueue.Add(u);
                        previous[u] = v;
                    }
                }
            }
            return previous;
        }
    }
}
