using System;
using System.Collections.Generic;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Algorithms.Flow;
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Algorithms.TopologicalSort;

namespace ZeGraphos.Extensions;

/// <summary>
/// Provides extension methods for graph algorithms with fluent API support.
/// Enables method chaining for discoverable and readable algorithm execution.
/// </summary>
public static class GraphExtensions
{
    /// <summary>
    /// Creates a shortest path algorithm builder for the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to perform shortest path algorithms on</param>
    /// <returns>A shortest path algorithm builder</returns>
    public static ShortestPathBuilder<T> ShortestPath<T>(this IGraph<T> graph)
        where T : notnull
    {
        return new ShortestPathBuilder<T>(graph);
    }

    /// <summary>
    /// Creates a maximum flow algorithm builder for the weighted directed graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TCapacity">The type of edge capacities</typeparam>
    /// <param name="graph">The weighted directed graph to perform flow algorithms on</param>
    /// <returns>A maximum flow algorithm builder</returns>
    public static MaximumFlowBuilder<T, TCapacity> MaximumFlow<T, TCapacity>(this IWeightedGraph<T, TCapacity> graph)
        where T : notnull
        where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
    {
        return new MaximumFlowBuilder<T, TCapacity>(graph);
    }

    /// <summary>
    /// Creates a minimum spanning tree algorithm builder for the weighted undirected graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted undirected graph to perform MST algorithms on</param>
    /// <returns>A minimum spanning tree algorithm builder</returns>
    public static MinimumSpanningTreeBuilder<T, TWeight> MinimumSpanningTree<T, TWeight>(this IWeightedGraph<T, TWeight> graph)
        where T : notnull
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new MinimumSpanningTreeBuilder<T, TWeight>(graph);
    }

    /// <summary>
    /// Creates a graph coloring algorithm builder for the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to perform coloring algorithms on</param>
    /// <returns>A graph coloring algorithm builder</returns>
    public static GraphColoringBuilder<T> Coloring<T>(this IGraph<T> graph)
        where T : notnull
    {
        return new GraphColoringBuilder<T>(graph);
    }

    /// <summary>
    /// Creates a topological sort algorithm builder for the directed graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The directed graph to perform topological sort on</param>
    /// <returns>A topological sort algorithm builder</returns>
    public static TopologicalSortBuilder<T> TopologicalSort<T>(this IGraph<T> graph)
        where T : notnull
    {
        return new TopologicalSortBuilder<T>(graph);
    }

    /// <summary>
    /// Checks if the graph is connected (all nodes reachable from any node).
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to check</param>
    /// <returns>True if the graph is connected, false otherwise</returns>
    public static bool IsConnected<T>(this IGraph<T> graph)
        where T : notnull
    {
        if (graph.NodeCount == 0 || graph.NodeCount == 1)
            return true;

        var nodes = graph.GetNodes().ToList();
        var visited = new HashSet<T>();
        var queue = new Queue<T>();

        queue.Enqueue(nodes[0]);
        visited.Add(nodes[0]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited.Count == graph.NodeCount;
    }

    /// <summary>
    /// Checks if the graph is a tree (connected and acyclic).
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to check</param>
    /// <returns>True if the graph is a tree, false otherwise</returns>
    public static bool IsTree<T>(this IGraph<T> graph)
        where T : notnull
    {
        // A tree must be connected and have exactly n-1 edges
        return graph.IsConnected() && graph.EdgeCount == graph.NodeCount - 1;
    }

    /// <summary>
    /// Checks if the graph is a complete graph (every node connected to every other node).
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to check</param>
    /// <returns>True if the graph is complete, false otherwise</returns>
    public static bool IsComplete<T>(this IGraph<T> graph)
        where T : notnull
    {
        if (graph.NodeCount <= 1)
            return true;

        var expectedEdgeCount = graph.IsDirected ? 
            graph.NodeCount * (graph.NodeCount - 1) : 
            graph.NodeCount * (graph.NodeCount - 1) / 2;

        return graph.EdgeCount == expectedEdgeCount;
    }

    /// <summary>
    /// Gets the density of the graph (ratio of actual edges to possible edges).
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to analyze</param>
    /// <returns>The density value between 0 and 1</returns>
    public static double GetDensity<T>(this IGraph<T> graph)
        where T : notnull
    {
        if (graph.NodeCount <= 1)
            return 0.0;

        var maxEdges = graph.IsDirected ? 
            graph.NodeCount * (graph.NodeCount - 1) : 
            graph.NodeCount * (graph.NodeCount - 1) / 2;

        return (double)graph.EdgeCount / maxEdges;
    }

    /// <summary>
    /// Gets the average degree of nodes in the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to analyze</param>
    /// <returns>The average degree</returns>
    public static double GetAverageDegree<T>(this IGraph<T> graph)
        where T : notnull
    {
        if (graph.NodeCount == 0)
            return 0.0;

        return (double)graph.EdgeCount * 2 / graph.NodeCount;
    }

    /// <summary>
    /// Creates a copy of the graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <param name="graph">The graph to copy</param>
    /// <returns>A new graph with the same nodes and edges</returns>
    public static IGraph<T> Copy<T>(this IGraph<T> graph)
        where T : notnull
    {
        IGraph<T> copy = graph.IsDirected ? 
            new Core.Implementations.DirectedGraph<T>() : 
            new Core.Implementations.UndirectedGraph<T>();

        // Add all nodes
        foreach (var node in graph.GetNodes())
        {
            copy.AddNode(node);
        }

        // Add all edges
        foreach (var edge in graph.GetEdges())
        {
            copy.AddEdge(edge.Source, edge.Target);
        }

        return copy;
    }

    /// <summary>
    /// Creates a copy of the weighted graph.
    /// </summary>
    /// <typeparam name="T">The type of nodes in the graph</typeparam>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted graph to copy</param>
    /// <returns>A new weighted graph with the same nodes and edges</returns>
    public static IWeightedGraph<T, TWeight> Copy<T, TWeight>(this IWeightedGraph<T, TWeight> graph)
        where T : notnull
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        IWeightedGraph<T, TWeight> copy = graph.IsDirected ? 
            new Core.Implementations.WeightedDirectedGraph<T, TWeight>() : 
            new Core.Implementations.WeightedUndirectedGraph<T, TWeight>();

        // Add all nodes
        foreach (var node in graph.GetNodes())
        {
            copy.AddNode(node);
        }

        // Add all weighted edges
        foreach (var edge in graph.GetWeightedEdges())
        {
            copy.AddEdge(edge.Source, edge.Target, edge.Weight);
        }

        return copy;
    }
}

/// <summary>
/// Builder for shortest path algorithms with fluent API.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class ShortestPathBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal ShortestPathBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Uses Dijkstra's algorithm for shortest path computation.
    /// </summary>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted graph</param>
    /// <returns>A Dijkstra algorithm builder</returns>
    public DijkstraBuilder<T, TWeight> Dijkstra<TWeight>(IWeightedGraph<T, TWeight> graph)
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new DijkstraBuilder<T, TWeight>(graph);
    }

    /// <summary>
    /// Uses BFS algorithm for unweighted shortest path computation.
    /// </summary>
    /// <returns>A BFS algorithm builder</returns>
    public BFSBuilder<T> BFS()
    {
        return new BFSBuilder<T>(_graph);
    }

    /// <summary>
    /// Uses A* algorithm for heuristic shortest path computation.
    /// </summary>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted graph</param>
    /// <param name="heuristic">The heuristic function</param>
    /// <returns>An A* algorithm builder</returns>
    public AStarBuilder<T, TWeight> AStar<TWeight>(IWeightedGraph<T, TWeight> graph, Func<T, T, TWeight> heuristic)
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new AStarBuilder<T, TWeight>(graph);
    }

    /// <summary>
    /// Uses Contraction Hierarchies algorithm for ultra-fast shortest path computation.
    /// </summary>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted graph</param>
    /// <returns>A Contraction Hierarchies algorithm builder</returns>
    public ContractionHierarchiesBuilder<T, TWeight> ContractionHierarchies<TWeight>(IWeightedGraph<T, TWeight> graph)
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new ContractionHierarchiesBuilder<T, TWeight>(graph);
    }

    /// <summary>
    /// Uses Transit Node Routing algorithm for fast shortest path computation.
    /// </summary>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted graph</param>
    /// <returns>A Transit Node Routing algorithm builder</returns>
    public TransitNodeRoutingBuilder<T, TWeight> TransitNodeRouting<TWeight>(IWeightedGraph<T, TWeight> graph)
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new TransitNodeRoutingBuilder<T, TWeight>(graph);
    }

    /// <summary>
    /// Uses Hub Labeling algorithm for ultra-fast point-to-point shortest path computation.
    /// </summary>
    /// <typeparam name="TWeight">The type of edge weights</typeparam>
    /// <param name="graph">The weighted graph</param>
    /// <returns>A Hub Labeling algorithm builder</returns>
    public HubLabelingBuilder<T, TWeight> HubLabeling<TWeight>(IWeightedGraph<T, TWeight> graph)
        where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
    {
        return new HubLabelingBuilder<T, TWeight>(graph);
    }
}

/// <summary>
/// Builder for maximum flow algorithms with fluent API.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public sealed class MaximumFlowBuilder<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    private readonly IWeightedGraph<T, TCapacity> _graph;

    internal MaximumFlowBuilder(IWeightedGraph<T, TCapacity> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Uses Ford-Fulkerson algorithm with DFS for augmenting paths.
    /// </summary>
    /// <returns>A Ford-Fulkerson algorithm builder</returns>
    public FordFulkersonBuilder<T, TCapacity> FordFulkerson()
    {
        return new FordFulkersonBuilder<T, TCapacity>(_graph);
    }

    /// <summary>
    /// Uses Edmonds-Karp algorithm with BFS for augmenting paths.
    /// </summary>
    /// <returns>An Edmonds-Karp algorithm builder</returns>
    public EdmondsKarpBuilder<T, TCapacity> EdmondsKarp()
    {
        return new EdmondsKarpBuilder<T, TCapacity>(_graph);
    }

    /// <summary>
    /// Uses Dinic's algorithm with level graphs.
    /// </summary>
    /// <returns>A Dinic algorithm builder</returns>
    public DinicBuilder<T, TCapacity> Dinic()
    {
        return new DinicBuilder<T, TCapacity>(_graph);
    }
}

/// <summary>
/// Builder for minimum spanning tree algorithms with fluent API.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class MinimumSpanningTreeBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal MinimumSpanningTreeBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Uses Kruskal's algorithm with Union-Find data structure.
    /// </summary>
    /// <returns>A Kruskal algorithm builder</returns>
    public KruskalBuilder<T, TWeight> Kruskal()
    {
        return new KruskalBuilder<T, TWeight>(_graph);
    }

    /// <summary>
    /// Uses Prim's algorithm with priority queue.
    /// </summary>
    /// <returns>A Prim algorithm builder</returns>
    public PrimBuilder<T, TWeight> Prim()
    {
        return new PrimBuilder<T, TWeight>(_graph);
    }

    /// <summary>
    /// Uses Borůvka's algorithm for parallel MST computation.
    /// </summary>
    /// <returns>A Borůvka algorithm builder</returns>
    public BoruvkaBuilder<T, TWeight> Boruvka()
    {
        return new BoruvkaBuilder<T, TWeight>(_graph);
    }
}

/// <summary>
/// Builder for graph coloring algorithms with fluent API.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class GraphColoringBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal GraphColoringBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Uses greedy coloring algorithm with various ordering strategies.
    /// </summary>
    /// <returns>A greedy coloring builder</returns>
    public GreedyColoringBuilder<T> Greedy()
    {
        return new GreedyColoringBuilder<T>(_graph);
    }

    /// <summary>
    /// Uses Welsh-Powell algorithm with degree ordering.
    /// </summary>
    /// <returns>A Welsh-Powell coloring builder</returns>
    public WelshPowellBuilder<T> WelshPowell()
    {
        return new WelshPowellBuilder<T>(_graph);
    }

    /// <summary>
    /// Uses DSATUR algorithm with saturation degree ordering.
    /// </summary>
    /// <returns>A DSATUR coloring builder</returns>
    public DSATURBuilder<T> DSATUR()
    {
        return new DSATURBuilder<T>(_graph);
    }
}

/// <summary>
/// Builder for topological sort algorithms with fluent API.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class TopologicalSortBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal TopologicalSortBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Uses Kahn's algorithm with queue-based processing.
    /// </summary>
    /// <returns>A Kahn algorithm builder</returns>
    public KahnBuilder<T> Kahn()
    {
        return new KahnBuilder<T>(_graph);
    }

    /// <summary>
    /// Uses DFS-based algorithm with reverse post-order.
    /// </summary>
    /// <returns>A DFS algorithm builder</returns>
    public DFSBuilder<T> DFS()
    {
        return new DFSBuilder<T>(_graph);
    }
}
