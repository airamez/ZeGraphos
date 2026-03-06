using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Core.Interfaces;

/// <summary>
/// Represents a weighted graph with nodes of type T and edge weights of type TWeight.
/// Extends IGraph&lt;T&gt; with weight-specific operations.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights (must be numeric)</typeparam>
public interface IWeightedGraph<T, TWeight> : IGraph<T>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Adds a weighted edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="weight">The weight of the edge</param>
    /// <returns>True if the edge was added, false if it already exists or nodes don't exist</returns>
    bool AddEdge(T source, T target, TWeight weight);

    /// <summary>
    /// Gets the weight of an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The weight of the edge</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the edge doesn't exist</exception>
    TWeight GetEdgeWeight(T source, T target);

    /// <summary>
    /// Sets the weight of an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="weight">The new weight of the edge</param>
    /// <exception cref="KeyNotFoundException">Thrown when the edge doesn't exist</exception>
    void SetEdgeWeight(T source, T target, TWeight weight);

    /// <summary>
    /// Tries to get the weight of an edge between two nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="weight">When this method returns, contains the weight if found; otherwise, the default value</param>
    /// <returns>True if the edge exists and weight was retrieved; false otherwise</returns>
    bool TryGetEdgeWeight(T source, T target, out TWeight weight);

    /// <summary>
    /// Gets all weighted edges in the graph.
    /// </summary>
    /// <returns>A read-only collection of all weighted edges</returns>
    IReadOnlyCollection<WeightedEdge<T, TWeight>> GetWeightedEdges();

    /// <summary>
    /// Gets the weighted neighbors of a node with their corresponding weights.
    /// </summary>
    /// <param name="node">The node to get weighted neighbors for</param>
    /// <returns>A read-only collection of weighted edges connecting to the node</returns>
    IReadOnlyCollection<WeightedEdge<T, TWeight>> GetWeightedNeighbors(T node);

    /// <summary>
    /// Gets the total weight of all edges in the graph.
    /// </summary>
    /// <returns>The sum of all edge weights</returns>
    TWeight GetTotalWeight();
}
