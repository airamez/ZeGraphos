using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Flow;

/// <summary>
/// Implementation of the Ford-Fulkerson algorithm using Depth-First Search for finding augmenting paths.
/// Computes maximum flow in a directed weighted graph with capacities.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public static class FordFulkerson<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    /// <summary>
    /// Computes the maximum flow from source to sink using Ford-Fulkerson algorithm.
    /// </summary>
    /// <param name="graph">The weighted directed graph representing the flow network</param>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <returns>The maximum flow result</returns>
    public static FlowResult<T, TCapacity> ComputeMaxFlow(IWeightedGraph<T, TCapacity> graph, T source, T sink)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");
        if (!graph.ContainsNode(sink))
            throw new KeyNotFoundException($"Sink node {sink} not found in graph.");

        var flow = new Dictionary<(T, T), TCapacity>();
        var iterations = 0;
        var augmentingPathsFound = 0;

        // Initialize flow to 0 for all edges
        foreach (var edge in graph.GetWeightedEdges())
        {
            flow[(edge.Source, edge.Target)] = GetMinValue();
        }

        while (true)
        {
            iterations++;

            // Find augmenting path using DFS
            var path = FindAugmentingPath(graph, flow, source, sink);
            if (path == null || path.Count == 0)
                break;

            augmentingPathsFound++;

            // Find bottleneck capacity in the path
            var bottleneck = GetBottleneckCapacity(graph, flow, path);

            // Augment flow along the path
            AugmentFlow(flow, path, bottleneck);
        }

        // Calculate max flow (total flow out of source)
        var maxFlow = GetMinValue();
        foreach (var edge in graph.GetWeightedNeighbors(source))
        {
            maxFlow = Add(maxFlow, flow[(source, edge.Target)]);
        }

        // Create flow edges dictionary
        var flowEdges = new Dictionary<WeightedEdge<T, TCapacity>, TCapacity>();
        foreach (var kvp in flow)
        {
            if (!kvp.Value.Equals(GetMinValue()))
            {
                var edge = new WeightedEdge<T, TCapacity>(kvp.Key.Item1, kvp.Key.Item2, 
                    graph.GetEdgeWeight(kvp.Key.Item1, kvp.Key.Item2));
                flowEdges[edge] = kvp.Value;
            }
        }

        return new FlowResult<T, TCapacity>(source, sink, maxFlow, flowEdges, 
            "Ford-Fulkerson", augmentingPathsFound, iterations);
    }

    /// <summary>
    /// Finds a minimum cut from the maximum flow result.
    /// </summary>
    /// <param name="graph">The original flow network</param>
    /// <param name="flowResult">The maximum flow result</param>
    /// <returns>The minimum cut</returns>
    public static MinimumCut<T, TCapacity> FindMinimumCut(IWeightedGraph<T, TCapacity> graph, FlowResult<T, TCapacity> flowResult)
    {
        var sourceSide = new HashSet<T>();
        var visited = new HashSet<T>();
        var flow = new Dictionary<(T, T), TCapacity>();

        // Reconstruct flow dictionary
        foreach (var kvp in flowResult.FlowEdges)
        {
            flow[(kvp.Key.Source, kvp.Key.Target)] = kvp.Value;
        }

        // Find all nodes reachable from source in residual graph
        DFSReachable(graph, flow, flowResult.Source, visited, sourceSide);

        // Find sink side (all nodes not in source side)
        var sinkSide = new HashSet<T>(graph.GetNodes().Where(n => !sourceSide.Contains(n)));

        // Find cut edges (edges from source side to sink side that are saturated)
        var cutEdges = new List<WeightedEdge<T, TCapacity>>();
        var cutCapacity = GetMinValue();

        foreach (var edge in graph.GetWeightedEdges())
        {
            if (sourceSide.Contains(edge.Source) && sinkSide.Contains(edge.Target))
            {
                cutEdges.Add(edge);
                cutCapacity = Add(cutCapacity, edge.Weight);
            }
        }

        return new MinimumCut<T, TCapacity>(sourceSide, sinkSide, cutCapacity, cutEdges);
    }

    private static List<T>? FindAugmentingPath(IWeightedGraph<T, TCapacity> graph, 
        Dictionary<(T, T), TCapacity> flow, T source, T sink)
    {
        var visited = new HashSet<T>();
        var path = new List<T>();
        var found = false;

        DFS(graph, flow, source, sink, visited, path, ref found);
        return found ? path : null;
    }

    private static void DFS(IWeightedGraph<T, TCapacity> graph, Dictionary<(T, T), TCapacity> flow, 
        T current, T sink, HashSet<T> visited, List<T> path, ref bool found)
    {
        if (found || visited.Contains(current))
            return;

        visited.Add(current);
        path.Add(current);

        if (current.Equals(sink))
        {
            found = true;
            return;
        }

        foreach (var edge in graph.GetWeightedNeighbors(current))
        {
            var residualCapacity = GetResidualCapacity(graph, flow, current, edge.Target);
            if (Compare(residualCapacity, GetMinValue()) > 0)
            {
                DFS(graph, flow, edge.Target, sink, visited, path, ref found);
                if (found)
                    return;
            }
        }

        if (!found)
            path.RemoveAt(path.Count - 1);
    }

    private static void DFSReachable(IWeightedGraph<T, TCapacity> graph, Dictionary<(T, T), TCapacity> flow, 
        T current, HashSet<T> visited, HashSet<T> reachable)
    {
        if (visited.Contains(current))
            return;

        visited.Add(current);
        reachable.Add(current);

        foreach (var edge in graph.GetWeightedNeighbors(current))
        {
            var residualCapacity = GetResidualCapacity(graph, flow, current, edge.Target);
            if (Compare(residualCapacity, GetMinValue()) > 0)
            {
                DFSReachable(graph, flow, edge.Target, visited, reachable);
            }
        }
    }

    private static TCapacity GetBottleneckCapacity(IWeightedGraph<T, TCapacity> graph, 
        Dictionary<(T, T), TCapacity> flow, List<T> path)
    {
        var bottleneck = GetMaxValue();

        for (int i = 0; i < path.Count - 1; i++)
        {
            var residualCapacity = GetResidualCapacity(graph, flow, path[i], path[i + 1]);
            if (Compare(residualCapacity, bottleneck) < 0)
                bottleneck = residualCapacity;
        }

        return bottleneck;
    }

    private static TCapacity GetResidualCapacity(IWeightedGraph<T, TCapacity> graph, 
        Dictionary<(T, T), TCapacity> flow, T source, T target)
    {
        var capacity = graph.GetEdgeWeight(source, target);
        var currentFlow = flow.ContainsKey((source, target)) ? flow[(source, target)] : GetMinValue();
        return Subtract(capacity, currentFlow);
    }

    private static void AugmentFlow(Dictionary<(T, T), TCapacity> flow, List<T> path, TCapacity bottleneck)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var source = path[i];
            var target = path[i + 1];

            // Forward edge
            var forwardKey = (source, target);
            flow[forwardKey] = flow.ContainsKey(forwardKey) ? Add(flow[forwardKey], bottleneck) : bottleneck;

            // Backward edge (for residual graph)
            var backwardKey = (target, source);
            flow[backwardKey] = flow.ContainsKey(backwardKey) ? Subtract(flow[backwardKey], bottleneck) : Subtract(GetMinValue(), bottleneck);
        }
    }

    private static TCapacity GetMinValue()
    {
        if (typeof(TCapacity) == typeof(double)) return (TCapacity)(object)0.0;
        if (typeof(TCapacity) == typeof(int)) return (TCapacity)(object)0;
        if (typeof(TCapacity) == typeof(float)) return (TCapacity)(object)0.0f;
        if (typeof(TCapacity) == typeof(decimal)) return (TCapacity)(object)0m;
        
        return default!;
    }

    private static TCapacity GetMaxValue()
    {
        if (typeof(TCapacity) == typeof(double)) return (TCapacity)(object)double.MaxValue;
        if (typeof(TCapacity) == typeof(int)) return (TCapacity)(object)int.MaxValue;
        if (typeof(TCapacity) == typeof(float)) return (TCapacity)(object)float.MaxValue;
        if (typeof(TCapacity) == typeof(decimal)) return (TCapacity)(object)decimal.MaxValue;
        
        dynamic maxValue = default(TCapacity);
        if (typeof(TCapacity).IsEnum)
        {
            var values = Enum.GetValues(typeof(TCapacity));
            maxValue = values.GetValue(values.Length - 1) ?? maxValue;
        }
        return maxValue;
    }

    private static TCapacity Add(TCapacity a, TCapacity b)
    {
        if (typeof(TCapacity) == typeof(double)) return (TCapacity)(object)((double)(object)a + (double)(object)b);
        if (typeof(TCapacity) == typeof(int)) return (TCapacity)(object)((int)(object)a + (int)(object)b);
        if (typeof(TCapacity) == typeof(float)) return (TCapacity)(object)((float)(object)a + (float)(object)b);
        if (typeof(TCapacity) == typeof(decimal)) return (TCapacity)(object)((decimal)(object)a + (decimal)(object)b);
        
        return (TCapacity)(object)((dynamic)a + (dynamic)b);
    }

    private static TCapacity Subtract(TCapacity a, TCapacity b)
    {
        if (typeof(TCapacity) == typeof(double)) return (TCapacity)(object)((double)(object)a - (double)(object)b);
        if (typeof(TCapacity) == typeof(int)) return (TCapacity)(object)((int)(object)a - (int)(object)b);
        if (typeof(TCapacity) == typeof(float)) return (TCapacity)(object)((float)(object)a - (float)(object)b);
        if (typeof(TCapacity) == typeof(decimal)) return (TCapacity)(object)((decimal)(object)a - (decimal)(object)b);
        
        return (TCapacity)(object)((dynamic)a - (dynamic)b);
    }

    private static int Compare(TCapacity a, TCapacity b)
    {
        if (typeof(TCapacity) == typeof(double)) return ((double)(object)a).CompareTo((double)(object)b);
        if (typeof(TCapacity) == typeof(int)) return ((int)(object)a).CompareTo((int)(object)b);
        if (typeof(TCapacity) == typeof(float)) return ((float)(object)a).CompareTo((float)(object)b);
        if (typeof(TCapacity) == typeof(decimal)) return ((decimal)(object)a).CompareTo((decimal)(object)b);
        
        return ((IComparable<TCapacity>)a).CompareTo(b);
    }
}
