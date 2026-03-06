using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Common;
using ZeGraphos.Core.Interfaces;

namespace ZeGraphos.Algorithms.TopologicalSort;

/// <summary>
/// Represents the result of a topological sort algorithm execution.
/// Contains the sorted order of nodes and algorithm metadata.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class TopologicalSortResult<T>
    where T : notnull
{
    /// <summary>
    /// Gets the topologically sorted order of nodes.
    /// </summary>
    public IReadOnlyList<T> SortedOrder { get; }

    /// <summary>
    /// Gets the algorithm used to compute the topological sort.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of nodes in the original graph.
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    /// Gets the number of nodes included in the sorted order.
    /// </summary>
    public int SortedNodeCount => SortedOrder.Count;

    /// <summary>
    /// Gets whether the graph is a DAG (no cycles detected).
    /// </summary>
    public bool IsDAG { get; }

    /// <summary>
    /// Gets whether all nodes were included in the sorted order.
    /// </summary>
    public bool IsComplete { get; }

    /// <summary>
    /// Gets the number of iterations performed during the algorithm execution.
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// Gets the nodes that were not included in the sorted order (due to cycles).
    /// </summary>
    public IReadOnlyCollection<T> UnsortedNodes { get; }

    /// <summary>
    /// Initializes a new instance of the TopologicalSortResult class for a successful sort.
    /// </summary>
    /// <param name="sortedOrder">The topologically sorted order of nodes</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="iterations">Number of iterations performed</param>
    public TopologicalSortResult(IEnumerable<T> sortedOrder, string algorithm, int iterations)
    {
        SortedOrder = new ReadOnlyCollection<T>(sortedOrder.ToList());
        Algorithm = algorithm;
        NodeCount = SortedOrder.Count;
        IsDAG = true;
        IsComplete = true;
        Iterations = iterations;
        UnsortedNodes = new ReadOnlyCollection<T>(new List<T>());
    }

    /// <summary>
    /// Initializes a new instance of the TopologicalSortResult class for a graph with cycles.
    /// </summary>
    /// <param name="sortedOrder">The topologically sorted order of nodes (partial)</param>
    /// <param name="unsortedNodes">Nodes that could not be sorted due to cycles</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="iterations">Number of iterations performed</param>
    public TopologicalSortResult(IEnumerable<T> sortedOrder, IEnumerable<T> unsortedNodes, string algorithm, int iterations)
    {
        SortedOrder = new ReadOnlyCollection<T>(sortedOrder.ToList());
        UnsortedNodes = new ReadOnlyCollection<T>(unsortedNodes.ToList());
        Algorithm = algorithm;
        NodeCount = SortedOrder.Count + UnsortedNodes.Count;
        IsDAG = false;
        IsComplete = false;
        Iterations = iterations;
    }

    /// <summary>
    /// Gets the position of a node in the sorted order.
    /// </summary>
    /// <param name="node">The node to get position for</param>
    /// <returns>The zero-based position of the node, or -1 if not found</returns>
    public int GetPosition(T node)
    {
        for (int i = 0; i < SortedOrder.Count; i++)
        {
            if (SortedOrder[i].Equals(node))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Gets whether a node appears before another node in the sorted order.
    /// </summary>
    /// <param name="node1">The first node</param>
    /// <param name="node2">The second node</param>
    /// <returns>True if node1 appears before node2, false otherwise</returns>
    public bool IsBefore(T node1, T node2)
    {
        var pos1 = GetPosition(node1);
        var pos2 = GetPosition(node2);
        
        return pos1 != -1 && pos2 != -1 && pos1 < pos2;
    }

    /// <summary>
    /// Gets the predecessors of a node (nodes that appear before it in the sorted order).
    /// </summary>
    /// <param name="node">The node to get predecessors for</param>
    /// <returns>A collection of predecessor nodes</returns>
    public IReadOnlyCollection<T> GetPredecessors(T node)
    {
        var position = GetPosition(node);
        if (position == -1)
            return new ReadOnlyCollection<T>(new List<T>());

        return new ReadOnlyCollection<T>(SortedOrder.Take(position).ToList());
    }

    /// <summary>
    /// Gets the successors of a node (nodes that appear after it in the sorted order).
    /// </summary>
    /// <param name="node">The node to get successors for</param>
    /// <returns>A collection of successor nodes</returns>
    public IReadOnlyCollection<T> GetSuccessors(T node)
    {
        var position = GetPosition(node);
        if (position == -1)
            return new ReadOnlyCollection<T>(new List<T>());

        return new ReadOnlyCollection<T>(SortedOrder.Skip(position + 1).ToList());
    }

    /// <summary>
    /// Validates that the sorted order is a valid topological sort for the given graph.
    /// </summary>
    /// <param name="graph">The graph to validate against</param>
    /// <returns>True if the sorted order is valid, false otherwise</returns>
    public bool Validate(IGraph<T> graph)
    {
        if (!graph.IsDirected)
            return false;

        // Check that all edges go from earlier to later nodes
        foreach (var edge in graph.GetEdges())
        {
            var sourcePos = GetPosition(edge.Source);
            var targetPos = GetPosition(edge.Target);

            if (sourcePos == -1 || targetPos == -1)
                continue; // Edge involves unsorted nodes

            if (sourcePos >= targetPos)
                return false; // Edge goes backwards
        }

        return true;
    }

    /// <summary>
    /// Returns a string representation of the topological sort result.
    /// </summary>
    /// <returns>A string showing the algorithm and result status</returns>
    public override string ToString()
    {
        if (IsDAG)
        {
            return $"Topological sort using {Algorithm}: {SortedNodeCount} nodes sorted " +
                   $"({Iterations} iterations)";
        }
        else
        {
            return $"Topological sort using {Algorithm}: {SortedNodeCount}/{NodeCount} nodes sorted, " +
                   $"{UnsortedNodes.Count} nodes in cycles ({Iterations} iterations)";
        }
    }
}

/// <summary>
/// Exception thrown when a cycle is detected during topological sorting.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class CycleDetectedException<T> : Exception
    where T : notnull
{
    /// <summary>
    /// Gets the nodes involved in the detected cycle.
    /// </summary>
    public IReadOnlyList<T> CycleNodes { get; }

    /// <summary>
    /// Initializes a new instance of the CycleDetectedException class.
    /// </summary>
    /// <param name="cycleNodes">The nodes involved in the cycle</param>
    /// <param name="message">The exception message</param>
    public CycleDetectedException(IReadOnlyList<T> cycleNodes, string message) : base(message)
    {
        CycleNodes = cycleNodes;
    }

    /// <summary>
    /// Initializes a new instance of the CycleDetectedException class.
    /// </summary>
    /// <param name="cycleNodes">The nodes involved in the cycle</param>
    public CycleDetectedException(IReadOnlyList<T> cycleNodes) : 
        base($"Cycle detected in graph containing nodes: {string.Join(" -> ", cycleNodes)}")
    {
        CycleNodes = cycleNodes;
    }
}
