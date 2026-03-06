using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.SpanningTree;

/// <summary>
/// Implementation of Borůvka's algorithm for parallel minimum spanning tree computation.
/// Grows MST by simultaneously adding the cheapest edge from each component.
/// Particularly suitable for parallel processing.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public static class Boruvka<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Computes a minimum spanning tree using Borůvka's algorithm.
    /// </summary>
    /// <param name="graph">The weighted undirected graph</param>
    /// <returns>The minimum spanning tree result</returns>
    public static MSTResult<T, TWeight> ComputeMST(IWeightedGraph<T, TWeight> graph)
    {
        if (graph.IsDirected)
            throw new ArgumentException("Borůvka's algorithm requires an undirected graph.");

        var unionFind = new UnionFind<T>();
        var mstEdges = new List<WeightedEdge<T, TWeight>>();
        var iterations = 0;
        var components = graph.NodeCount;

        // Initialize Union-Find with all nodes
        foreach (var node in graph.GetNodes())
        {
            unionFind.MakeSet(node);
        }

        while (components > 1)
        {
            iterations++;

            // Find cheapest edge for each component
            var cheapestEdges = new Dictionary<T, WeightedEdge<T, TWeight>>();

            foreach (var edge in graph.GetWeightedEdges())
            {
                var root1 = unionFind.Find(edge.Source);
                var root2 = unionFind.Find(edge.Target);

                if (root1.Equals(root2))
                    continue; // Edge is within the same component

                // Update cheapest edge for component 1
                if (!cheapestEdges.ContainsKey(root1) || Compare(edge.Weight, cheapestEdges[root1].Weight) < 0)
                {
                    cheapestEdges[root1] = edge;
                }

                // Update cheapest edge for component 2
                if (!cheapestEdges.ContainsKey(root2) || Compare(edge.Weight, cheapestEdges[root2].Weight) < 0)
                {
                    cheapestEdges[root2] = edge;
                }
            }

            // Add cheapest edges to MST and merge components
            var edgesToAdd = new HashSet<WeightedEdge<T, TWeight>>();
            foreach (var edge in cheapestEdges.Values)
            {
                var root1 = unionFind.Find(edge.Source);
                var root2 = unionFind.Find(edge.Target);

                if (!root1.Equals(root2))
                {
                    edgesToAdd.Add(edge);
                    mstEdges.Add(edge);
                    unionFind.Union(edge.Source, edge.Target);
                    components--;
                }
            }

            // If no edges were added in this iteration, we're done
            if (edgesToAdd.Count == 0)
                break;
        }

        return new MSTResult<T, TWeight>(mstEdges, "Borůvka", graph.NodeCount, iterations);
    }

    /// <summary>
    /// Computes a minimum spanning forest for a disconnected graph.
    /// </summary>
    /// <param name="graph">The weighted undirected graph (may be disconnected)</param>
    /// <returns>The minimum spanning forest result</returns>
    public static ForestResult<T, TWeight> ComputeMinimumForest(IWeightedGraph<T, TWeight> graph)
    {
        if (graph.IsDirected)
            throw new ArgumentException("Borůvka's algorithm requires an undirected graph.");

        var trees = new List<MSTResult<T, TWeight>>();
        var visited = new HashSet<T>();

        foreach (var node in graph.GetNodes())
        {
            if (!visited.Contains(node))
            {
                // Find connected component using BFS
                var component = GetConnectedComponent(graph, node, visited);
                
                if (component.Count == 1)
                {
                    // Single node component
                    trees.Add(new MSTResult<T, TWeight>(Array.Empty<WeightedEdge<T, TWeight>>(), "Borůvka", 1, 0));
                }
                else
                {
                    // Create a subgraph for this component and compute MST
                    var componentMST = ComputeMSTForComponent(graph, component);
                    trees.Add(componentMST);
                }
            }
        }

        return new ForestResult<T, TWeight>(trees, "Borůvka");
    }

    private static HashSet<T> GetConnectedComponent(IWeightedGraph<T, TWeight> graph, T startNode, HashSet<T> globalVisited)
    {
        var component = new HashSet<T>();
        var queue = new Queue<T>();

        queue.Enqueue(startNode);
        component.Add(startNode);
        globalVisited.Add(startNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!globalVisited.Contains(neighbor))
                {
                    globalVisited.Add(neighbor);
                    component.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return component;
    }

    private static MSTResult<T, TWeight> ComputeMSTForComponent(IWeightedGraph<T, TWeight> graph, HashSet<T> component)
    {
        var unionFind = new UnionFind<T>();
        var mstEdges = new List<WeightedEdge<T, TWeight>>();
        var iterations = 0;
        var components = component.Count;

        // Initialize Union-Find with all nodes in component
        foreach (var node in component)
        {
            unionFind.MakeSet(node);
        }

        while (components > 1)
        {
            iterations++;

            // Find cheapest edge for each component
            var cheapestEdges = new Dictionary<T, WeightedEdge<T, TWeight>>();

            foreach (var edge in graph.GetWeightedEdges().Where(e => component.Contains(e.Source) && component.Contains(e.Target)))
            {
                var root1 = unionFind.Find(edge.Source);
                var root2 = unionFind.Find(edge.Target);

                if (root1.Equals(root2))
                    continue; // Edge is within the same component

                // Update cheapest edge for component 1
                if (!cheapestEdges.ContainsKey(root1) || Compare(edge.Weight, cheapestEdges[root1].Weight) < 0)
                {
                    cheapestEdges[root1] = edge;
                }

                // Update cheapest edge for component 2
                if (!cheapestEdges.ContainsKey(root2) || Compare(edge.Weight, cheapestEdges[root2].Weight) < 0)
                {
                    cheapestEdges[root2] = edge;
                }
            }

            // Add cheapest edges to MST and merge components
            var edgesToAdd = new HashSet<WeightedEdge<T, TWeight>>();
            foreach (var edge in cheapestEdges.Values)
            {
                var root1 = unionFind.Find(edge.Source);
                var root2 = unionFind.Find(edge.Target);

                if (!root1.Equals(root2))
                {
                    edgesToAdd.Add(edge);
                    mstEdges.Add(edge);
                    unionFind.Union(edge.Source, edge.Target);
                    components--;
                }
            }

            // If no edges were added in this iteration, we're done
            if (edgesToAdd.Count == 0)
                break;
        }

        return new MSTResult<T, TWeight>(mstEdges, "Borůvka", component.Count, iterations);
    }

    private static int Compare(TWeight a, TWeight b)
    {
        if (typeof(TWeight) == typeof(double)) return ((double)(object)a).CompareTo((double)(object)b);
        if (typeof(TWeight) == typeof(int)) return ((int)(object)a).CompareTo((int)(object)b);
        if (typeof(TWeight) == typeof(float)) return ((float)(object)a).CompareTo((float)(object)b);
        if (typeof(TWeight) == typeof(decimal)) return ((decimal)(object)a).CompareTo((decimal)(object)b);
        
        return ((IComparable<TWeight>)a).CompareTo(b);
    }

    /// <summary>
    /// Union-Find (Disjoint Set Union) data structure for Borůvka's algorithm.
    /// </summary>
    /// <typeparam name="TNode">The type of nodes</typeparam>
    private class UnionFind<TNode>
        where TNode : notnull
    {
        private readonly Dictionary<TNode, TNode> _parent;
        private readonly Dictionary<TNode, int> _rank;

        public UnionFind()
        {
            _parent = new Dictionary<TNode, TNode>();
            _rank = new Dictionary<TNode, int>();
        }

        public void MakeSet(TNode node)
        {
            _parent[node] = node;
            _rank[node] = 0;
        }

        public TNode Find(TNode node)
        {
            if (!_parent.ContainsKey(node))
                throw new KeyNotFoundException($"Node {node} not found in Union-Find structure.");

            // Path compression
            if (!node.Equals(_parent[node]))
            {
                _parent[node] = Find(_parent[node]);
            }

            return _parent[node];
        }

        public void Union(TNode node1, TNode node2)
        {
            var root1 = Find(node1);
            var root2 = Find(node2);

            if (root1.Equals(root2))
                return; // Already in the same set

            // Union by rank
            if (_rank[root1] < _rank[root2])
            {
                _parent[root1] = root2;
            }
            else if (_rank[root1] > _rank[root2])
            {
                _parent[root2] = root1;
            }
            else
            {
                _parent[root2] = root1;
                _rank[root1]++;
            }
        }
    }
}
