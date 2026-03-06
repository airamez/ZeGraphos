using System;
using System.Collections.Generic;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Implementations;

namespace ZeGraphos.Core.Common;

/// <summary>
/// Provides a fluent API for building different types of graphs.
/// Supports method chaining for discoverable and readable graph construction.
/// </summary>
public static class GraphBuilder
{
    /// <summary>
    /// Creates a new directed graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <returns>A new directed graph instance</returns>
    public static DirectedGraph<T> CreateDirected<T>()
        where T : notnull
    {
        return new DirectedGraph<T>();
    }

    /// <summary>
    /// Creates a new undirected graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <returns>A new undirected graph instance</returns>
    public static UndirectedGraph<T> CreateUndirected<T>()
        where T : notnull
    {
        return new UndirectedGraph<T>();
    }

    /// <summary>
    /// Creates a new weighted directed graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <returns>A new weighted directed graph instance</returns>
    public static WeightedDirectedGraph<T, TWeight> CreateWeightedDirected<T, TWeight>()
        where T : notnull
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new WeightedDirectedGraph<T, TWeight>();
    }

    /// <summary>
    /// Creates a new weighted undirected graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <returns>A new weighted undirected graph instance</returns>
    public static WeightedUndirectedGraph<T, TWeight> CreateWeightedUndirected<T, TWeight>()
        where T : notnull
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new WeightedUndirectedGraph<T, TWeight>();
    }
}

/// <summary>
/// Provides extension methods for fluent graph building operations.
/// </summary>
public static class GraphBuilderExtensions
{
    /// <summary>
    /// Adds multiple nodes to the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to add nodes to</param>
    /// <param name="nodes">The nodes to add</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> AddNodes<T>(this IGraph<T> graph, params T[] nodes)
        where T : notnull
    {
        foreach (var node in nodes)
        {
            graph.AddNode(node);
        }
        return graph;
    }

    /// <summary>
    /// Adds multiple nodes to the graph from an enumerable collection.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to add nodes to</param>
    /// <param name="nodes">The nodes to add</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> AddNodes<T>(this IGraph<T> graph, IEnumerable<T> nodes)
        where T : notnull
    {
        foreach (var node in nodes)
        {
            graph.AddNode(node);
        }
        return graph;
    }

    /// <summary>
    /// Adds multiple edges to the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to add edges to</param>
    /// <param name="edges">The edges to add as tuples of (source, target)</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> AddEdges<T>(this IGraph<T> graph, params (T source, T target)[] edges)
        where T : notnull
    {
        foreach (var (source, target) in edges)
        {
            graph.AddEdge(source, target);
        }
        return graph;
    }

    /// <summary>
    /// Adds multiple weighted edges to the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The graph to add edges to</param>
    /// <param name="edges">The weighted edges to add as tuples of (source, target, weight)</param>
    /// <returns>The graph for method chaining</returns>
    public static IWeightedGraph<T, TWeight> AddEdges<T, TWeight>(this IWeightedGraph<T, TWeight> graph, params (T source, T target, TWeight weight)[] edges)
        where T : notnull
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        foreach (var (source, target, weight) in edges)
        {
            graph.AddEdge(source, target, weight);
        }
        return graph;
    }

    /// <summary>
    /// Creates a complete graph where every node is connected to every other node.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to make complete</param>
    /// <param name="nodes">The nodes to include in the complete graph</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> MakeComplete<T>(this IGraph<T> graph, params T[] nodes)
        where T : notnull
    {
        // Add all nodes
        graph.AddNodes(nodes);

        // Add all possible edges
        for (int i = 0; i < nodes.Length; i++)
        {
            for (int j = 0; j < nodes.Length; j++)
            {
                if (i != j) // Skip self-loops unless explicitly allowed
                {
                    graph.AddEdge(nodes[i], nodes[j]);
                }
            }
        }

        return graph;
    }

    /// <summary>
    /// Creates a complete weighted graph where every node is connected to every other node with the specified weight.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The graph to make complete</param>
    /// <param name="weight">The weight for all edges</param>
    /// <param name="nodes">The nodes to include in the complete graph</param>
    /// <returns>The graph for method chaining</returns>
    public static IWeightedGraph<T, TWeight> MakeComplete<T, TWeight>(this IWeightedGraph<T, TWeight> graph, TWeight weight, params T[] nodes)
        where T : notnull
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        // Add all nodes
        graph.AddNodes(nodes);

        // Add all possible weighted edges
        for (int i = 0; i < nodes.Length; i++)
        {
            for (int j = 0; j < nodes.Length; j++)
            {
                if (i != j) // Skip self-loops unless explicitly allowed
                {
                    graph.AddEdge(nodes[i], nodes[j], weight);
                }
            }
        }

        return graph;
    }

    /// <summary>
    /// Creates a cycle graph with the specified nodes in order.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to make into a cycle</param>
    /// <param name="nodes">The nodes to include in the cycle (in order)</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> MakeCycle<T>(this IGraph<T> graph, params T[] nodes)
        where T : notnull
    {
        if (nodes.Length < 2)
            return graph;

        // Add all nodes
        graph.AddNodes(nodes);

        // Add edges to form a cycle
        for (int i = 0; i < nodes.Length; i++)
        {
            var nextIndex = (i + 1) % nodes.Length;
            graph.AddEdge(nodes[i], nodes[nextIndex]);
        }

        return graph;
    }

    /// <summary>
    /// Creates a path graph with the specified nodes in order.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to make into a path</param>
    /// <param name="nodes">The nodes to include in the path (in order)</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> MakePath<T>(this IGraph<T> graph, params T[] nodes)
        where T : notnull
    {
        if (nodes.Length < 2)
            return graph;

        // Add all nodes
        graph.AddNodes(nodes);

        // Add edges to form a path
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            graph.AddEdge(nodes[i], nodes[i + 1]);
        }

        return graph;
    }

    /// <summary>
    /// Creates a star graph with the specified center node and leaf nodes.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to make into a star</param>
    /// <param name="center">The center node of the star</param>
    /// <param name="leaves">The leaf nodes of the star</param>
    /// <returns>The graph for method chaining</returns>
    public static IGraph<T> MakeStar<T>(this IGraph<T> graph, T center, params T[] leaves)
        where T : notnull
    {
        // Add center and leaf nodes
        graph.AddNode(center);
        graph.AddNodes(leaves);

        // Connect center to all leaves
        foreach (var leaf in leaves)
        {
            graph.AddEdge(center, leaf);
        }

        return graph;
    }
}
