using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Core.Implementations;

/// <summary>
/// Implementation of a weighted directed graph using adjacency list representation.
/// Provides O(1) node lookup and efficient weighted edge management for directed graphs.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public class WeightedDirectedGraph<T, TWeight> : IWeightedGraph<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly Dictionary<T, Dictionary<T, TWeight>> _adjacencyList;
    private readonly Dictionary<T, int> _inDegree;

    /// <summary>
    /// Initializes a new instance of the WeightedDirectedGraph class.
    /// </summary>
    public WeightedDirectedGraph()
    {
        _adjacencyList = new Dictionary<T, Dictionary<T, TWeight>>();
        _inDegree = new Dictionary<T, int>();
    }

    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    public int NodeCount => _adjacencyList.Count;

    /// <summary>
    /// Gets the number of edges in the graph.
    /// </summary>
    public int EdgeCount => _adjacencyList.Values.Sum(neighbors => neighbors.Count);

    /// <summary>
    /// Gets whether the graph is directed.
    /// </summary>
    public bool IsDirected => true;

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
        _inDegree[node] = 0;
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

        // Remove all outgoing edges
        var outgoingEdges = _adjacencyList[node].ToList();
        foreach (var kvp in outgoingEdges)
        {
            RemoveEdge(node, kvp.Key);
        }

        // Remove all incoming edges
        var incomingNodes = _adjacencyList.Keys.Where(source => _adjacencyList[source].ContainsKey(node)).ToList();
        foreach (var source in incomingNodes)
        {
            RemoveEdge(source, node);
        }

        // Remove the node
        _adjacencyList.Remove(node);
        _inDegree.Remove(node);
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
        // Ensure both nodes exist
        if (!ContainsNode(source))
            AddNode(source);
        if (!ContainsNode(target))
            AddNode(target);

        // Add the edge if it doesn't already exist
        if (!_adjacencyList[source].ContainsKey(target))
        {
            _adjacencyList[source][target] = weight;
            _inDegree[target]++;
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
        _inDegree[target]--;
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
            throw new KeyNotFoundException($"Edge from {source} to {target} not found in graph.");

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
            throw new KeyNotFoundException($"Edge from {source} to {target} not found in graph.");

        _adjacencyList[source][target] = weight;
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
        foreach (var kvp in _adjacencyList)
        {
            foreach (var target in kvp.Value.Keys)
            {
                edges.Add(new Edge<T>(kvp.Key, target));
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
        foreach (var kvp in _adjacencyList)
        {
            foreach (var targetKvp in kvp.Value)
            {
                edges.Add(new WeightedEdge<T, TWeight>(kvp.Key, targetKvp.Key, targetKvp.Value));
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
    /// For directed graphs, this returns the out-degree.
    /// </summary>
    /// <param name="node">The node to get degree for</param>
    /// <returns>The out-degree of the node</returns>
    public int GetDegree(T node)
    {
        if (!ContainsNode(node))
            throw new KeyNotFoundException($"Node {node} not found in graph.");

        return _adjacencyList[node].Count;
    }

    /// <summary>
    /// Gets the in-degree of a node (number of incoming edges).
    /// </summary>
    /// <param name="node">The node to get in-degree for</param>
    /// <returns>The in-degree of the node</returns>
    public int GetInDegree(T node)
    {
        if (!ContainsNode(node))
            throw new KeyNotFoundException($"Node {node} not found in graph.");

        return _inDegree[node];
    }

    /// <summary>
    /// Gets the total weight of all edges in the graph.
    /// </summary>
    /// <returns>The sum of all edge weights</returns>
    public TWeight GetTotalWeight()
    {
        if (typeof(TWeight) == typeof(double))
        {
            double total = 0;
            foreach (var kvp in _adjacencyList)
            {
                foreach (var weight in kvp.Value.Values)
                {
                    total += Convert.ToDouble(weight);
                }
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(int))
        {
            int total = 0;
            foreach (var kvp in _adjacencyList)
            {
                foreach (var weight in kvp.Value.Values)
                {
                    total += Convert.ToInt32(weight);
                }
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(float))
        {
            float total = 0;
            foreach (var kvp in _adjacencyList)
            {
                foreach (var weight in kvp.Value.Values)
                {
                    total += Convert.ToSingle(weight);
                }
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else
        {
            // For other numeric types, use dynamic
            dynamic total = default(TWeight);
            foreach (var kvp in _adjacencyList)
            {
                foreach (var weight in kvp.Value.Values)
                {
                    total += (dynamic)weight;
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
        _inDegree.Clear();
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
