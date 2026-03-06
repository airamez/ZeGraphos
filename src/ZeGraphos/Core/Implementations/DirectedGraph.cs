using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Core.Implementations;

/// <summary>
/// Implementation of a directed graph using adjacency list representation.
/// Provides O(1) node lookup and efficient edge management for directed graphs.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public class DirectedGraph<T> : IGraph<T>
    where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _adjacencyList;
    private readonly Dictionary<T, int> _inDegree;

    /// <summary>
    /// Initializes a new instance of the DirectedGraph class.
    /// </summary>
    public DirectedGraph()
    {
        _adjacencyList = new Dictionary<T, HashSet<T>>();
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
        foreach (var target in outgoingEdges)
        {
            RemoveEdge(node, target);
        }

        // Remove all incoming edges
        var incomingNodes = _adjacencyList.Keys.Where(source => _adjacencyList[source].Contains(node)).ToList();
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
    /// Adds an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge was added, false if it already exists or nodes don't exist</returns>
    public bool AddEdge(T source, T target)
    {
        // Ensure both nodes exist
        if (!ContainsNode(source))
            AddNode(source);
        if (!ContainsNode(target))
            AddNode(target);

        // Add the edge if it doesn't already exist
        if (_adjacencyList[source].Add(target))
        {
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
        foreach (var kvp in _adjacencyList)
        {
            foreach (var target in kvp.Value)
            {
                edges.Add(new Edge<T>(kvp.Key, target));
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
