using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Core.Implementations;

/// <summary>
/// Implementation of a weighted undirected graph using adjacency list representation.
/// Provides O(1) node lookup and efficient weighted edge management for undirected graphs.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public class WeightedUndirectedGraph<T, TWeight> : IWeightedGraph<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly Dictionary<T, Dictionary<T, TWeight>> _adjacencyList;

    /// <summary>
    /// Initializes a new instance of the WeightedUndirectedGraph class.
    /// </summary>
    public WeightedUndirectedGraph()
    {
        _adjacencyList = new Dictionary<T, Dictionary<T, TWeight>>();
    }

    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    public int NodeCount => _adjacencyList.Count;

    /// <summary>
    /// Gets the number of edges in the graph.
    /// </summary>
    public int EdgeCount => _adjacencyList.Values.Sum(neighbors => neighbors.Count) / 2;

    /// <summary>
    /// Gets whether the graph is directed.
    /// </summary>
    public bool IsDirected => false;

    /// <summary>
    /// Gets whether the graph is weighted.
    /// </summary>
    public bool IsWeighted => true;

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add</param>
    /// <returns>True if the node was added, false if it already exists</returns>
    public bool AddNode(T node)
    {
        if (_adjacencyList.ContainsKey(node))
            return false;

        _adjacencyList[node] = new Dictionary<T, TWeight>();
        return true;
    }

    /// <summary>
    /// Removes a node from the graph, along with all its edges.
    /// </summary>
    /// <param name="node">The node to remove</param>
    /// <returns>True if the node was removed, false if it doesn't exist</returns>
    public bool RemoveNode(T node)
    {
        if (!_adjacencyList.ContainsKey(node))
            return false;

        // Remove all edges connected to this node
        var neighbors = _adjacencyList[node].Keys.ToList();
        foreach (var neighbor in neighbors)
        {
            RemoveEdge(node, neighbor);
        }

        // Remove the node
        _adjacencyList.Remove(node);
        return true;
    }

    /// <summary>
    /// Adds an edge between two nodes (unweighted - not supported for weighted graphs).
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>Always throws NotSupportedException for weighted graphs</returns>
    /// <exception cref="NotSupportedException">Always thrown for weighted graphs</exception>
    public bool AddEdge(T source, T target)
    {
        throw new NotSupportedException("Use AddEdge(T source, T target, TWeight weight) for weighted graphs.");
    }

    /// <summary>
    /// Adds a weighted edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="weight">The weight of the edge</param>
    /// <returns>True if the edge was added, false if it already exists or nodes don't exist</returns>
    public bool AddEdge(T source, T target, TWeight weight)
    {
        // For undirected graphs, self-loops are typically not allowed
        if (EqualityComparer<T>.Default.Equals(source, target))
            return false;

        // Ensure both nodes exist
        if (!ContainsNode(source))
            AddNode(source);
        if (!ContainsNode(target))
            AddNode(target);

        // Add the edge in both directions if it doesn't already exist
        if (!_adjacencyList[source].ContainsKey(target))
        {
            _adjacencyList[source][target] = weight;
            _adjacencyList[target][source] = weight;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge was removed, false if it doesn't exist</returns>
    public bool RemoveEdge(T source, T target)
    {
        if (!ContainsEdge(source, target))
            return false;

        _adjacencyList[source].Remove(target);
        _adjacencyList[target].Remove(source);
        return true;
    }

    /// <summary>
    /// Checks if the graph contains a specific node.
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns>True if the node exists, false otherwise</returns>
    public bool ContainsNode(T node) => _adjacencyList.ContainsKey(node);

    /// <summary>
    /// Checks if the graph contains an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge exists, false otherwise</returns>
    public bool ContainsEdge(T source, T target) =>
        _adjacencyList.ContainsKey(source) && _adjacencyList[source].ContainsKey(target);

    /// <summary>
    /// Gets the weight of an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The weight of the edge</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the edge doesn't exist</exception>
    public TWeight GetEdgeWeight(T source, T target)
    {
        if (!ContainsEdge(source, target))
            throw new KeyNotFoundException($"Edge between {source} and {target} not found in graph.");

        return _adjacencyList[source][target];
    }

    /// <summary>
    /// Sets the weight of an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="weight">The new weight of the edge</param>
    /// <exception cref="KeyNotFoundException">Thrown when the edge doesn't exist</exception>
    public void SetEdgeWeight(T source, T target, TWeight weight)
    {
        if (!ContainsEdge(source, target))
            throw new KeyNotFoundException($"Edge between {source} and {target} not found in graph.");

        _adjacencyList[source][target] = weight;
        _adjacencyList[target][source] = weight;
    }

    /// <summary>
    /// Tries to get the weight of an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="weight">When this method returns, contains the weight if found; otherwise, the default value</param>
    /// <returns>True if the edge exists and weight was retrieved; false otherwise</returns>
    public bool TryGetEdgeWeight(T source, T target, out TWeight weight)
    {
        if (ContainsEdge(source, target))
        {
            weight = _adjacencyList[source][target];
            return true;
        }

        weight = default!;
        return false;
    }

    /// <summary>
    /// Gets all nodes in the graph.
    /// </summary>
    /// <returns>A read-only collection of all nodes</returns>
    public IReadOnlyCollection<T> GetNodes() => new ReadOnlyCollection<T>(_adjacencyList.Keys.ToList());

    /// <summary>
    /// Gets all edges in the graph.
    /// </summary>
    /// <returns>A read-only collection of all edges</returns>
    public IReadOnlyCollection<Edge<T>> GetEdges()
    {
        var edges = new List<Edge<T>>();
        var processed = new HashSet<(T, T)>();

        foreach (var kvp in _adjacencyList)
        {
            foreach (var target in kvp.Value.Keys)
            {
                var edge = (kvp.Key, target);
                var reverseEdge = (target, kvp.Key);

                // Only add each edge once (avoid duplicates in undirected graph)
                if (!processed.Contains(edge) && !processed.Contains(reverseEdge))
                {
                    edges.Add(new Edge<T>(kvp.Key, target));
                    processed.Add(edge);
                }
            }
        }
        return new ReadOnlyCollection<Edge<T>>(edges);
    }

    /// <summary>
    /// Gets all weighted edges in the graph.
    /// </summary>
    /// <returns>A read-only collection of all weighted edges</returns>
    public IReadOnlyCollection<WeightedEdge<T, TWeight>> GetWeightedEdges()
    {
        var edges = new List<WeightedEdge<T, TWeight>>();
        var processed = new HashSet<(T, T)>();

        foreach (var kvp in _adjacencyList)
        {
            foreach (var targetKvp in kvp.Value)
            {
                var edge = (kvp.Key, targetKvp.Key);
                var reverseEdge = (targetKvp.Key, kvp.Key);

                // Only add each edge once (avoid duplicates in undirected graph)
                if (!processed.Contains(edge) && !processed.Contains(reverseEdge))
                {
                    edges.Add(new WeightedEdge<T, TWeight>(kvp.Key, targetKvp.Key, targetKvp.Value));
                    processed.Add(edge);
                }
            }
        }
        return new ReadOnlyCollection<WeightedEdge<T, TWeight>>(edges);
    }

    /// <summary>
    /// Gets the neighbors of a node.
    /// </summary>
    /// <param name="node">The node to get neighbors for</param>
    /// <returns>A read-only collection of neighboring nodes</returns>
    public IReadOnlyCollection<T> GetNeighbors(T node)
    {
        if (!ContainsNode(node))
            throw new KeyNotFoundException($"Node {node} not found in graph.");

        return new ReadOnlyCollection<T>(_adjacencyList[node].Keys.ToList());
    }

    /// <summary>
    /// Gets the weighted neighbors of a node with their corresponding weights.
    /// </summary>
    /// <param name="node">The node to get weighted neighbors for</param>
    /// <returns>A read-only collection of weighted edges connecting to the node</returns>
    public IReadOnlyCollection<WeightedEdge<T, TWeight>> GetWeightedNeighbors(T node)
    {
        if (!ContainsNode(node))
            throw new KeyNotFoundException($"Node {node} not found in graph.");

        var weightedEdges = new List<WeightedEdge<T, TWeight>>();
        foreach (var kvp in _adjacencyList[node])
        {
            weightedEdges.Add(new WeightedEdge<T, TWeight>(node, kvp.Key, kvp.Value));
        }
        return new ReadOnlyCollection<WeightedEdge<T, TWeight>>(weightedEdges);
    }

    /// <summary>
    /// Gets the degree (number of edges) of a node.
    /// </summary>
    /// <param name="node">The node to get degree for</param>
    /// <returns>The degree of the node</returns>
    public int GetDegree(T node)
    {
        if (!ContainsNode(node))
            throw new KeyNotFoundException($"Node {node} not found in graph.");

        return _adjacencyList[node].Count;
    }

    /// <summary>
    /// Gets the total weight of all edges in the graph.
    /// </summary>
    /// <returns>The sum of all edge weights (each edge counted once)</returns>
    public TWeight GetTotalWeight()
    {
        if (typeof(TWeight) == typeof(double))
        {
            double total = 0;
            var processed = new HashSet<(T, T)>();

            foreach (var kvp in _adjacencyList)
            {
                foreach (var targetKvp in kvp.Value)
                {
                    var edge = (kvp.Key, targetKvp.Key);
                    var reverseEdge = (targetKvp.Key, kvp.Key);

                    // Only count each edge once (avoid duplicates in undirected graph)
                    if (!processed.Contains(edge) && !processed.Contains(reverseEdge))
                    {
                        total += Convert.ToDouble(targetKvp.Value);
                        processed.Add(edge);
                    }
                }
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(int))
        {
            int total = 0;
            var processed = new HashSet<(T, T)>();

            foreach (var kvp in _adjacencyList)
            {
                foreach (var targetKvp in kvp.Value)
                {
                    var edge = (kvp.Key, targetKvp.Key);
                    var reverseEdge = (targetKvp.Key, kvp.Key);

                    // Only count each edge once (avoid duplicates in undirected graph)
                    if (!processed.Contains(edge) && !processed.Contains(reverseEdge))
                    {
                        total += Convert.ToInt32(targetKvp.Value);
                        processed.Add(edge);
                    }
                }
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(float))
        {
            float total = 0;
            var processed = new HashSet<(T, T)>();

            foreach (var kvp in _adjacencyList)
            {
                foreach (var targetKvp in kvp.Value)
                {
                    var edge = (kvp.Key, targetKvp.Key);
                    var reverseEdge = (targetKvp.Key, kvp.Key);

                    // Only count each edge once (avoid duplicates in undirected graph)
                    if (!processed.Contains(edge) && !processed.Contains(reverseEdge))
                    {
                        total += Convert.ToSingle(targetKvp.Value);
                        processed.Add(edge);
                    }
                }
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else
        {
            // For other numeric types, use dynamic
            dynamic total = default(TWeight);
            var processed = new HashSet<(T, T)>();

            foreach (var kvp in _adjacencyList)
            {
                foreach (var targetKvp in kvp.Value)
                {
                    var edge = (kvp.Key, targetKvp.Key);
                    var reverseEdge = (targetKvp.Key, kvp.Key);

                    // Only count each edge once (avoid duplicates in undirected graph)
                    if (!processed.Contains(edge) && !processed.Contains(reverseEdge))
                    {
                        total += (dynamic)targetKvp.Value;
                        processed.Add(edge);
                    }
                }
            }
            return total;
        }
    }

    /// <summary>
    /// Clears all nodes and edges from the graph.
    /// </summary>
    public void Clear()
    {
        _adjacencyList.Clear();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the nodes in the graph.
    /// </summary>
    /// <returns>An enumerator for the nodes</returns>
    public IEnumerator<T> GetEnumerator() => _adjacencyList.Keys.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the nodes in the graph.
    /// </summary>
    /// <returns>An enumerator for the nodes</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
