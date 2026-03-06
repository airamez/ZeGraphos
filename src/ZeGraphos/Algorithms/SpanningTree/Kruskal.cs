using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.SpanningTree;

/// <summary>
/// Implementation of Kruskal's algorithm using Union-Find data structure.
/// Finds a minimum spanning tree by greedily adding the cheapest edges that don't create cycles.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public static class Kruskal<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Computes a minimum spanning tree using Kruskal's algorithm.
    /// </summary>
    /// <param name="graph">The weighted undirected graph</param>
    /// <returns>The minimum spanning tree result</returns>
    public static MSTResult<T, TWeight> ComputeMST(IWeightedGraph<T, TWeight> graph)
    {
        if (graph.IsDirected)
            throw new ArgumentException("Kruskal's algorithm requires an undirected graph.");

        var edges = graph.GetWeightedEdges().OrderBy(e => e.Weight).ToList();
        var unionFind = new UnionFind<T>();
        var mstEdges = new List<WeightedEdge<T, TWeight>>();
        var iterations = 0;

        // Initialize Union-Find with all nodes
        foreach (var node in graph.GetNodes())
        {
            unionFind.MakeSet(node);
        }

        // Process edges in order of increasing weight
        foreach (var edge in edges)
        {
            iterations++;

            // Check if adding this edge creates a cycle
            if (!EqualityComparer<T>.Default.Equals(unionFind.Find(edge.Source), unionFind.Find(edge.Target)))
            {
                mstEdges.Add(edge);
                unionFind.Union(edge.Source, edge.Target);

                // Stop when we have n-1 edges (for connected graph)
                if (mstEdges.Count == graph.NodeCount - 1)
                    break;
            }
        }

        return new MSTResult<T, TWeight>(mstEdges, "Kruskal", graph.NodeCount, iterations);
    }

    /// <summary>
    /// Computes a minimum spanning forest for a disconnected graph.
    /// </summary>
    /// <param name="graph">The weighted undirected graph (may be disconnected)</param>
    /// <returns>The minimum spanning forest result</returns>
    public static ForestResult<T, TWeight> ComputeMinimumForest(IWeightedGraph<T, TWeight> graph)
    {
        if (graph.IsDirected)
            throw new ArgumentException("Kruskal's algorithm requires an undirected graph.");

        var edges = graph.GetWeightedEdges().OrderBy(e => e.Weight).ToList();
        var unionFind = new UnionFind<T>();
        var forestEdges = new Dictionary<T, List<WeightedEdge<T, TWeight>>>();
        var iterations = 0;

        // Initialize Union-Find with all nodes
        foreach (var node in graph.GetNodes())
        {
            unionFind.MakeSet(node);
            forestEdges[node] = new List<WeightedEdge<T, TWeight>>();
        }

        // Process edges in order of increasing weight
        foreach (var edge in edges)
        {
            iterations++;

            // Check if adding this edge creates a cycle
            if (!EqualityComparer<T>.Default.Equals(unionFind.Find(edge.Source), unionFind.Find(edge.Target)))
            {
                var root = unionFind.Find(edge.Source);
                forestEdges[root].Add(edge);
                unionFind.Union(edge.Source, edge.Target);

                // Update forest edges mapping after union
                var newRoot = unionFind.Find(edge.Source);
                if (!EqualityComparer<T>.Default.Equals(newRoot, root))
                {
                    forestEdges[newRoot] = forestEdges[root];
                    forestEdges.Remove(root);
                }
            }
        }

        // Create MST results for each tree in the forest
        var trees = new List<MSTResult<T, TWeight>>();
        var processedNodes = new HashSet<T>();

        foreach (var kvp in forestEdges.Where(kvp => kvp.Value.Count > 0))
        {
            var treeNodes = GetTreeNodes(kvp.Value);
            var treeNodeCount = treeNodes.Count;

            // Only process nodes that haven't been included in another tree
            if (!treeNodes.Any(processedNodes.Contains))
            {
                trees.Add(new MSTResult<T, TWeight>(kvp.Value, "Kruskal", treeNodeCount, iterations));
                foreach (var node in treeNodes)
                {
                    processedNodes.Add(node);
                }
            }
        }

        // Add isolated nodes as single-node trees
        foreach (var node in graph.GetNodes())
        {
            if (!processedNodes.Contains(node))
            {
                trees.Add(new MSTResult<T, TWeight>(Array.Empty<WeightedEdge<T, TWeight>>(), "Kruskal", 1, 0));
                processedNodes.Add(node);
            }
        }

        return new ForestResult<T, TWeight>(trees, "Kruskal");
    }

    private static HashSet<T> GetTreeNodes(List<WeightedEdge<T, TWeight>> edges)
    {
        var nodes = new HashSet<T>();
        foreach (var edge in edges)
        {
            nodes.Add(edge.Source);
            nodes.Add(edge.Target);
        }
        return nodes;
    }

    /// <summary>
    /// Union-Find (Disjoint Set Union) data structure for Kruskal's algorithm.
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
