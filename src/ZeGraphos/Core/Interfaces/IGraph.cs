using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Core.Interfaces;

/// <summary>
/// Represents a graph data structure with nodes of type T.
/// Provides basic operations for graph manipulation and traversal.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public interface IGraph<T> : IEnumerable<T>
    where T : notnull
{
    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    int NodeCount { get; }

    /// <summary>
    /// Gets the number of edges in the graph.
    /// </summary>
    int EdgeCount { get; }

    /// <summary>
    /// Gets whether the graph is directed.
    /// </summary>
    bool IsDirected { get; }

    /// <summary>
    /// Gets whether the graph is weighted.
    /// </summary>
    bool IsWeighted { get; }

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add</param>
    /// <returns>True if the node was added, false if it already exists</returns>
    bool AddNode(T node);

    /// <summary>
    /// Removes a node from the graph, along with all its edges.
    /// </summary>
    /// <param name="node">The node to remove</param>
    /// <returns>True if the node was removed, false if it doesn't exist</returns>
    bool RemoveNode(T node);

    /// <summary>
    /// Adds an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge was added, false if it already exists or nodes don't exist</returns>
    bool AddEdge(T source, T target);

    /// <summary>
    /// Removes an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge was removed, false if it doesn't exist</returns>
    bool RemoveEdge(T source, T target);

    /// <summary>
    /// Checks if the graph contains a specific node.
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns>True if the node exists, false otherwise</returns>
    bool ContainsNode(T node);

    /// <summary>
    /// Checks if the graph contains an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the edge exists, false otherwise</returns>
    bool ContainsEdge(T source, T target);

    /// <summary>
    /// Gets all nodes in the graph.
    /// </summary>
    /// <returns>A read-only collection of all nodes</returns>
    IReadOnlyCollection<T> GetNodes();

    /// <summary>
    /// Gets all edges in the graph.
    /// </summary>
    /// <returns>A read-only collection of all edges</returns>
    IReadOnlyCollection<Edge<T>> GetEdges();

    /// <summary>
    /// Gets the neighbors of a node.
    /// </summary>
    /// <param name="node">The node to get neighbors for</param>
    /// <returns>A read-only collection of neighboring nodes</returns>
    IReadOnlyCollection<T> GetNeighbors(T node);

    /// <summary>
    /// Gets the degree (number of edges) of a node.
    /// </summary>
    /// <param name="node">The node to get degree for</param>
    /// <returns>The degree of the node</returns>
    int GetDegree(T node);

    /// <summary>
    /// Clears all nodes and edges from the graph.
    /// </summary>
    void Clear();
}
