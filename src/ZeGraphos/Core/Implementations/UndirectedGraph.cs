using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Core.Implementations;

/// <summary>
/// Implementation of an undirected graph using adjacency list representation.
/// Provides O(1) node lookup and efficient edge management for undirected graphs.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public class UndirectedGraph<T> : IGraph<T>
    where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _adjacencyList;

    /// <summary>
    /// Initializes a new instance of the UndirectedGraph class.
    /// </summary>
    public UndirectedGraph()
    {
        _adjacencyList = new Dictionary<T, HashSet<T>>();
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
    public bool IsWeighted => false;

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add</param>
    /// <returns>True if the node was added, false if it already exists</returns>
    public bool AddNode(T node)
    {
        if (_adjacencyList.ContainsKey(node))
            return false;

        _adjacencyList[node] = new HashSet<T>();
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
        var neighbors = _adjacencyList[node].ToList();
        foreach (var neighbor in neighbors)
        {
            RemoveEdge(node, neighbor);
        }

        // Remove the node
        _adjacencyList.Remove(node);
        return true;
    }

    /// <summary>
    /// Adds an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge was added, false if it already exists or nodes don't exist</returns>
    public bool AddEdge(T source, T target)
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
        if (_adjacencyList[source].Add(target))
        {
            _adjacencyList[target].Add(source);
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
        _adjacencyList.ContainsKey(source) && _adjacencyList[source].Contains(target);

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
            foreach (var target in kvp.Value)
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
    /// Gets the neighbors of a node.
    /// </summary>
    /// <param name="node">The node to get neighbors for</param>
    /// <returns>A read-only collection of neighboring nodes</returns>
    public IReadOnlyCollection<T> GetNeighbors(T node)
    {
        if (!ContainsNode(node))
            throw new KeyNotFoundException($"Node {node} not found in graph.");

        return new ReadOnlyCollection<T>(_adjacencyList[node].ToList());
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
