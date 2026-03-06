using System;
using System.Collections.Generic;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Algorithms.Flow;
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Algorithms.TopologicalSort;

namespace ZeGraphos.Extensions;

#region Shortest Path Algorithm Builders

/// <summary>
/// Builder for Dijkstra's shortest path algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class DijkstraBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal DijkstraBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Finds the shortest path from source to target.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> From(T source, T target)
    {
        return Dijkstra<T, TWeight>.FindShortestPath(_graph, source, target);
    }

    /// <summary>
    /// Finds shortest paths from source to all reachable nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <returns>All paths result</returns>
    public AllPathsResult<T, TWeight> From(T source)
    {
        return Dijkstra<T, TWeight>.FindAllShortestPaths(_graph, source);
    }
}

/// <summary>
/// Builder for BFS shortest path algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class BFSBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal BFSBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Finds the shortest path from source to target.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, int> From(T source, T target)
    {
        return BFS<T>.FindShortestPath(_graph, source, target);
    }

    /// <summary>
    /// Finds shortest paths from source to all reachable nodes.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <returns>All paths result</returns>
    public AllPathsResult<T, int> From(T source)
    {
        return BFS<T>.FindAllShortestPaths(_graph, source);
    }

    /// <summary>
    /// Performs BFS traversal from source.
    /// </summary>
    /// <param name="source">The starting node</param>
    /// <returns>Nodes in BFS visitation order</returns>
    public IReadOnlyList<T> Traverse(T source)
    {
        return BFS<T>.Traverse(_graph, source);
    }

    /// <summary>
    /// Checks if two nodes are connected.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if connected, false otherwise</returns>
    public bool AreConnected(T source, T target)
    {
        return BFS<T>.AreConnected(_graph, source, target);
    }

    /// <summary>
    /// Finds all connected components.
    /// </summary>
    /// <returns>List of connected components</returns>
    public IReadOnlyList<IReadOnlyList<T>> FindComponents()
    {
        return BFS<T>.FindConnectedComponents(_graph);
    }
}

/// <summary>
/// Builder for A* shortest path algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class AStarBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal AStarBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Finds the shortest path from source to target using the specified heuristic.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="heuristic">The heuristic function</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> From(T source, T target, AStar<T, TWeight>.HeuristicFunction heuristic)
    {
        return AStar<T, TWeight>.FindShortestPath(_graph, source, target, heuristic);
    }

    /// <summary>
    /// Uses Manhattan distance heuristic (for grid coordinates).
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="coordinates">Node coordinate mapping</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> WithManhattanHeuristic(T source, T target, Dictionary<T, (int x, int y)> coordinates)
    {
        var heuristic = AStar<T, TWeight>.CreateManhattanHeuristic(coordinates);
        return From(source, target, heuristic);
    }

    /// <summary>
    /// Uses Euclidean distance heuristic (for 2D coordinates).
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="coordinates">Node coordinate mapping</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> WithEuclideanHeuristic(T source, T target, Dictionary<T, (double x, double y)> coordinates)
    {
        var heuristic = AStar<T, TWeight>.CreateEuclideanHeuristic(coordinates);
        return From(source, target, heuristic);
    }
}

#endregion

#region Maximum Flow Algorithm Builders

/// <summary>
/// Builder for Ford-Fulkerson maximum flow algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public sealed class FordFulkersonBuilder<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    private readonly IWeightedGraph<T, TCapacity> _graph;

    internal FordFulkersonBuilder(IWeightedGraph<T, TCapacity> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes maximum flow from source to sink.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>The maximum flow result</returns>
    public FlowResult<T, TCapacity> From(T source, T sink)
    {
        return FordFulkerson<T, TCapacity>.ComputeMaxFlow(_graph, source, sink);
    }

    /// <summary>
    /// Computes maximum flow and finds minimum cut.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>A tuple containing the flow result and minimum cut</returns>
    public (FlowResult<T, TCapacity> Flow, MinimumCut<T, TCapacity> Cut) FromWithCut(T source, T sink)
    {
        var flow = From(source, sink);
        var cut = FordFulkerson<T, TCapacity>.FindMinimumCut(_graph, flow);
        return (flow, cut);
    }
}

/// <summary>
/// Builder for Edmonds-Karp maximum flow algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public sealed class EdmondsKarpBuilder<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    private readonly IWeightedGraph<T, TCapacity> _graph;

    internal EdmondsKarpBuilder(IWeightedGraph<T, TCapacity> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes maximum flow from source to sink.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>The maximum flow result</returns>
    public FlowResult<T, TCapacity> From(T source, T sink)
    {
        return EdmondsKarp<T, TCapacity>.ComputeMaxFlow(_graph, source, sink);
    }

    /// <summary>
    /// Computes maximum flow and finds minimum cut.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>A tuple containing the flow result and minimum cut</returns>
    public (FlowResult<T, TCapacity> Flow, MinimumCut<T, TCapacity> Cut) FromWithCut(T source, T sink)
    {
        var flow = From(source, sink);
        var cut = EdmondsKarp<T, TCapacity>.FindMinimumCut(_graph, flow);
        return (flow, cut);
    }
}

/// <summary>
/// Builder for Dinic's maximum flow algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public sealed class DinicBuilder<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    private readonly IWeightedGraph<T, TCapacity> _graph;

    internal DinicBuilder(IWeightedGraph<T, TCapacity> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes maximum flow from source to sink.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>The maximum flow result</returns>
    public FlowResult<T, TCapacity> From(T source, T sink)
    {
        return Dinic<T, TCapacity>.ComputeMaxFlow(_graph, source, sink);
    }

    /// <summary>
    /// Computes maximum flow and finds minimum cut.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>A tuple containing the flow result and minimum cut</returns>
    public (FlowResult<T, TCapacity> Flow, MinimumCut<T, TCapacity> Cut) FromWithCut(T source, T sink)
    {
        var flow = From(source, sink);
        var cut = Dinic<T, TCapacity>.FindMinimumCut(_graph, flow);
        return (flow, cut);
    }
}

#endregion

#region Minimum Spanning Tree Algorithm Builders

/// <summary>
/// Builder for Kruskal's minimum spanning tree algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class KruskalBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal KruskalBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes the minimum spanning tree.
    /// </summary>
    /// <returns>The MST result</returns>
    public MSTResult<T, TWeight> Compute()
    {
        return Kruskal<T, TWeight>.ComputeMST(_graph);
    }

    /// <summary>
    /// Computes the minimum spanning forest for disconnected graphs.
    /// </summary>
    /// <returns>The minimum spanning forest result</returns>
    public ForestResult<T, TWeight> ComputeForest()
    {
        return Kruskal<T, TWeight>.ComputeMinimumForest(_graph);
    }
}

/// <summary>
/// Builder for Prim's minimum spanning tree algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class PrimBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal PrimBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes the minimum spanning tree starting from the first node.
    /// </summary>
    /// <returns>The MST result</returns>
    public MSTResult<T, TWeight> Compute()
    {
        return Prim<T, TWeight>.ComputeMST(_graph);
    }

    /// <summary>
    /// Computes the minimum spanning tree starting from a specific node.
    /// </summary>
    /// <param name="startNode">The starting node</param>
    /// <returns>The MST result</returns>
    public MSTResult<T, TWeight> From(T startNode)
    {
        return Prim<T, TWeight>.ComputeMST(_graph, startNode);
    }

    /// <summary>
    /// Computes the minimum spanning forest for disconnected graphs.
    /// </summary>
    /// <returns>The minimum spanning forest result</returns>
    public ForestResult<T, TWeight> ComputeForest()
    {
        return Prim<T, TWeight>.ComputeMinimumForest(_graph);
    }
}

/// <summary>
/// Builder for Borůvka's minimum spanning tree algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class BoruvkaBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal BoruvkaBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes the minimum spanning tree.
    /// </summary>
    /// <returns>The MST result</returns>
    public MSTResult<T, TWeight> Compute()
    {
        return Boruvka<T, TWeight>.ComputeMST(_graph);
    }

    /// <summary>
    /// Computes the minimum spanning forest for disconnected graphs.
    /// </summary>
    /// <returns>The minimum spanning forest result</returns>
    public ForestResult<T, TWeight> ComputeForest()
    {
        return Boruvka<T, TWeight>.ComputeMinimumForest(_graph);
    }
}

#endregion

#region Graph Coloring Algorithm Builders

/// <summary>
/// Builder for greedy graph coloring algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class GreedyColoringBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal GreedyColoringBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes graph coloring with specified ordering and strategy.
    /// </summary>
    /// <param name="ordering">The node ordering strategy</param>
    /// <param name="strategy">The coloring strategy</param>
    /// <returns>The coloring result</returns>
    public ColoringResult<T> WithOrdering(ColoringOrdering ordering = ColoringOrdering.DegreeDescending, 
        ColoringStrategy strategy = ColoringStrategy.FirstAvailable)
    {
        return GreedyColoring<T>.ColorGraph(_graph, ordering, strategy);
    }

    /// <summary>
    /// Computes graph coloring with optimal settings for the graph type.
    /// </summary>
    /// <returns>The coloring result</returns>
    public ColoringResult<T> Optimal()
    {
        return GreedyColoring<T>.ColorGraphOptimal(_graph);
    }

    /// <summary>
    /// Improves an existing coloring.
    /// </summary>
    /// <param name="initialColoring">The initial coloring to improve</param>
    /// <param name="maxIterations">Maximum improvement iterations</param>
    /// <returns>The improved coloring result</returns>
    public ColoringResult<T> Improve(ColoringResult<T> initialColoring, int maxIterations = 100)
    {
        return GreedyColoring<T>.ImproveColoring(_graph, initialColoring, maxIterations);
    }
}

/// <summary>
/// Builder for Welsh-Powell graph coloring algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class WelshPowellBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal WelshPowellBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes graph coloring using Welsh-Powell algorithm.
    /// </summary>
    /// <returns>The coloring result</returns>
    public ColoringResult<T> Compute()
    {
        return WelshPowell<T>.ColorGraph(_graph);
    }

    /// <summary>
    /// Computes graph coloring using optimized Welsh-Powell algorithm.
    /// </summary>
    /// <param name="useOptimization">Whether to use optimization heuristics</param>
    /// <returns>The coloring result</returns>
    public ColoringResult<T> Optimized(bool useOptimization = true)
    {
        return WelshPowell<T>.ColorGraphOptimized(_graph, useOptimization);
    }

    /// <summary>
    /// Estimates the chromatic number bounds.
    /// </summary>
    /// <returns>A tuple with lower and upper bounds</returns>
    public (int LowerBound, int UpperBound) EstimateChromaticNumber()
    {
        return WelshPowell<T>.EstimateChromaticNumber(_graph);
    }
}

/// <summary>
/// Builder for DSATUR graph coloring algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class DSATURBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal DSATURBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Computes graph coloring using DSATUR algorithm.
    /// </summary>
    /// <returns>The coloring result</returns>
    public ColoringResult<T> Compute()
    {
        return DSATUR<T>.ColorGraph(_graph);
    }

    /// <summary>
    /// Computes graph coloring using advanced DSATUR with tie-breaking.
    /// </summary>
    /// <param name="useDegreeTieBreaker">Whether to use degree as tie-breaker</param>
    /// <param name="useRandomTieBreaker">Whether to use random tie-breaking</param>
    /// <returns>The coloring result</returns>
    public ColoringResult<T> Advanced(bool useDegreeTieBreaker = true, bool useRandomTieBreaker = false)
    {
        return DSATUR<T>.ColorGraphAdvanced(_graph, useDegreeTieBreaker, useRandomTieBreaker);
    }

    /// <summary>
    /// Estimates the chromatic number bounds.
    /// </summary>
    /// <returns>A tuple with lower and upper bounds</returns>
    public (int LowerBound, int UpperBound) EstimateChromaticNumber()
    {
        return DSATUR<T>.EstimateChromaticNumber(_graph);
    }
}

#endregion

#region Topological Sort Algorithm Builders

/// <summary>
/// Builder for Kahn's topological sort algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class KahnBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal KahnBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Performs topological sort using Kahn's algorithm.
    /// </summary>
    /// <returns>The topological sort result</returns>
    public TopologicalSortResult<T> Sort()
    {
        return Kahn<T>.Sort(_graph);
    }

    /// <summary>
    /// Performs deterministic topological sort using priority queue.
    /// </summary>
    /// <param name="comparer">Optional node comparer</param>
    /// <returns>The topological sort result</returns>
    public TopologicalSortResult<T> Deterministic(IComparer<T>? comparer = null)
    {
        return Kahn<T>.SortDeterministic(_graph, comparer);
    }

    /// <summary>
    /// Checks if the graph contains cycles.
    /// </summary>
    /// <returns>True if cycles exist, false otherwise</returns>
    public bool HasCycle()
    {
        return Kahn<T>.HasCycle(_graph);
    }

    /// <summary>
    /// Finds all source nodes (nodes with no incoming edges).
    /// </summary>
    /// <returns>Collection of source nodes</returns>
    public IReadOnlyCollection<T> FindSourceNodes()
    {
        return Kahn<T>.FindSourceNodes(_graph);
    }

    /// <summary>
    /// Finds all sink nodes (nodes with no outgoing edges).
    /// </summary>
    /// <returns>Collection of sink nodes</returns>
    public IReadOnlyCollection<T> FindSinkNodes()
    {
        return Kahn<T>.FindSinkNodes(_graph);
    }
}

/// <summary>
/// Builder for DFS-based topological sort algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class DFSBuilder<T>
    where T : notnull
{
    private readonly IGraph<T> _graph;

    internal DFSBuilder(IGraph<T> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Performs topological sort using DFS-based algorithm.
    /// </summary>
    /// <returns>The topological sort result</returns>
    public TopologicalSortResult<T> Sort()
    {
        return DFSTopologicalSort<T>.Sort(_graph);
    }

    /// <summary>
    /// Performs strict topological sort that throws on cycles.
    /// </summary>
    /// <returns>The topological sort result</returns>
    /// <exception cref="CycleDetectedException{T}">Thrown when cycles are detected</exception>
    public TopologicalSortResult<T> Strict()
    {
        return DFSTopologicalSort<T>.SortStrict(_graph);
    }

    /// <summary>
    /// Checks if the graph contains cycles.
    /// </summary>
    /// <returns>True if cycles exist, false otherwise</returns>
    public bool HasCycle()
    {
        return DFSTopologicalSort<T>.HasCycle(_graph);
    }

    /// <summary>
    /// Finds a cycle in the graph.
    /// </summary>
    /// <returns>List of nodes forming a cycle, or empty if no cycle</returns>
    public IReadOnlyList<T> FindCycle()
    {
        return DFSTopologicalSort<T>.FindCycle(_graph);
    }

    /// <summary>
    /// Performs topological sort starting from specific nodes.
    /// </summary>
    /// <param name="startNodes">The starting nodes</param>
    /// <returns>The topological sort result</returns>
    public TopologicalSortResult<T> FromNodes(IEnumerable<T> startNodes)
    {
        return DFSTopologicalSort<T>.SortFromNodes(_graph, startNodes);
    }
}

#region Advanced Shortest Path Algorithm Builders

/// <summary>
/// Builder for Contraction Hierarchies shortest path algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class ContractionHierarchiesBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal ContractionHierarchiesBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Finds the shortest path from source to target using Contraction Hierarchies.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> From(T source, T target)
    {
        return ContractionHierarchies<T, TWeight>.FindShortestPath(_graph, source, target);
    }
}

/// <summary>
/// Builder for Transit Node Routing shortest path algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class TransitNodeRoutingBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal TransitNodeRoutingBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Finds the shortest path from source to target using Transit Node Routing.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> From(T source, T target)
    {
        return TransitNodeRouting<T, TWeight>.FindShortestPath(_graph, source, target);
    }
}

/// <summary>
/// Builder for Hub Labeling shortest path algorithm.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class HubLabelingBuilder<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    private readonly IWeightedGraph<T, TWeight> _graph;

    internal HubLabelingBuilder(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Finds the shortest path from source to target using Hub Labeling.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result</returns>
    public PathResult<T, TWeight> From(T source, T target)
    {
        return HubLabeling<T, TWeight>.FindShortestPath(_graph, source, target);
    }
}

#endregion


#endregion
