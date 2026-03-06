using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Flow;

/// <summary>
/// Implementation of Dinic's algorithm using level graphs and blocking flow.
/// One of the fastest maximum flow algorithms with O(EV²) time complexity.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public static class Dinic<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    /// <summary>
    /// Computes the maximum flow from source to sink using Dinic's algorithm.
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
        var level = new Dictionary<T, int>();
        var iterations = 0;
        var augmentingPathsFound = 0;

        // Initialize flow to 0 for all edges
        foreach (var edge in graph.GetWeightedEdges())
        {
            flow[(edge.Source, edge.Target)] = GetMinValue();
        }

        while (BuildLevelGraph(graph, flow, source, sink, level))
        {
            iterations++;

            // Send blocking flow
            while (true)
            {
                var pathFlow = SendBlockingFlow(graph, flow, source, sink, level, new HashSet<T>());
                if (Compare(pathFlow, GetMinValue()) == 0)
                    break;

                augmentingPathsFound++;
            }

            // Clear level graph for next iteration
            level.Clear();
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
            "Dinic", augmentingPathsFound, iterations);
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

        // Build level graph from source to find reachable nodes
        BuildLevelGraph(graph, flow, flowResult.Source, flowResult.Sink, new Dictionary<T, int>());

        // Find all nodes reachable from source in residual graph using BFS
        BFSReachable(graph, flow, flowResult.Source, visited, sourceSide);

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

    private static bool BuildLevelGraph(IWeightedGraph<T, TCapacity> graph, 
        Dictionary<(T, T), TCapacity> flow, T source, T sink, Dictionary<T, int> level)
    {
        var queue = new Queue<T>();
        var visited = new HashSet<T>();

        queue.Enqueue(source);
        visited.Add(source);
        level[source] = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var edge in graph.GetWeightedNeighbors(current))
            {
                var residualCapacity = GetResidualCapacity(graph, flow, current, edge.Target);
                if (Compare(residualCapacity, GetMinValue()) > 0 && !visited.Contains(edge.Target))
                {
                    visited.Add(edge.Target);
                    level[edge.Target] = level[current] + 1;
                    queue.Enqueue(edge.Target);
                }
            }
        }

        return level.ContainsKey(sink);
    }

    private static TCapacity SendBlockingFlow(IWeightedGraph<T, TCapacity> graph, 
        Dictionary<(T, T), TCapacity> flow, T current, T sink, 
        Dictionary<T, int> level, HashSet<T> visited)
    {
        if (current.Equals(sink))
            return GetMaxValue(); // Found path to sink

        visited.Add(current);

        foreach (var edge in graph.GetWeightedNeighbors(current))
        {
            var residualCapacity = GetResidualCapacity(graph, flow, current, edge.Target);
            
            if (Compare(residualCapacity, GetMinValue()) > 0 && 
                level[edge.Target] == level[current] + 1 && 
                !visited.Contains(edge.Target))
            {
                var pathFlow = SendBlockingFlow(graph, flow, edge.Target, sink, level, visited);
                
                if (Compare(pathFlow, GetMinValue()) > 0)
                {
                    var actualFlow = Compare(pathFlow, residualCapacity) < 0 ? pathFlow : residualCapacity;
                    
                    // Augment flow
                    var forwardKey = (current, edge.Target);
                    flow[forwardKey] = flow.ContainsKey(forwardKey) ? Add(flow[forwardKey], actualFlow) : actualFlow;

                    var backwardKey = (edge.Target, current);
                    flow[backwardKey] = flow.ContainsKey(backwardKey) ? Subtract(flow[backwardKey], actualFlow) : Subtract(GetMinValue(), actualFlow);

                    return actualFlow;
                }
            }
        }

        return GetMinValue(); // No path found
    }

    private static void BFSReachable(IWeightedGraph<T, TCapacity> graph, Dictionary<(T, T), TCapacity> flow, 
        T source, HashSet<T> visited, HashSet<T> reachable)
    {
        var queue = new Queue<T>();
        queue.Enqueue(source);
        visited.Add(source);
        reachable.Add(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Forward edges with residual capacity
            foreach (var edge in graph.GetWeightedNeighbors(current))
            {
                var residualCapacity = GetResidualCapacity(graph, flow, current, edge.Target);
                if (Compare(residualCapacity, GetMinValue()) > 0 && !visited.Contains(edge.Target))
                {
                    visited.Add(edge.Target);
                    reachable.Add(edge.Target);
                    queue.Enqueue(edge.Target);
                }
            }

            // Backward edges with flow
            foreach (var node in graph.GetNodes())
            {
                if (graph.ContainsEdge(node, current))
                {
                    var reverseFlow = flow.ContainsKey((node, current)) ? flow[(node, current)] : GetMinValue();
                    if (Compare(reverseFlow, GetMinValue()) > 0 && !visited.Contains(node))
                    {
                        visited.Add(node);
                        reachable.Add(node);
                        queue.Enqueue(node);
                    }
                }
            }
        }
    }

    private static TCapacity GetResidualCapacity(IWeightedGraph<T, TCapacity> graph, 
        Dictionary<(T, T), TCapacity> flow, T source, T target)
    {
        if (graph.ContainsEdge(source, target))
        {
            var capacity = graph.GetEdgeWeight(source, target);
            var currentFlow = flow.ContainsKey((source, target)) ? flow[(source, target)] : GetMinValue();
            return Subtract(capacity, currentFlow);
        }
        else
        {
            // Backward edge - residual capacity is the current flow
            return flow.ContainsKey((target, source)) ? flow[(target, source)] : GetMinValue();
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
