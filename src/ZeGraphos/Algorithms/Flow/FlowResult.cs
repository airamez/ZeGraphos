using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Flow;

/// <summary>
/// Represents the result of a maximum flow algorithm execution.
/// Contains the maximum flow value and the flow assignment for each edge.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public sealed class FlowResult<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    /// <summary>
    /// Gets the source node of the flow network.
    /// </summary>
    public T Source { get; }

    /// <summary>
    /// Gets the sink node of the flow network.
    /// </summary>
    public T Sink { get; }

    /// <summary>
    /// Gets the maximum flow value from source to sink.
    /// </summary>
    public TCapacity MaxFlow { get; }

    /// <summary>
    /// Gets the flow assignment for each edge in the network.
    /// </summary>
    public IReadOnlyDictionary<WeightedEdge<T, TCapacity>, TCapacity> FlowEdges { get; }

    /// <summary>
    /// Gets the algorithm used to compute the maximum flow.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of augmenting paths found during the algorithm execution.
    /// </summary>
    public int AugmentingPathsFound { get; }

    /// <summary>
    /// Gets the number of iterations performed during the algorithm execution.
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// Initializes a new instance of the FlowResult class.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="sink">The sink node</param>
    /// <param name="maxFlow">The maximum flow value</param>
    /// <param name="flowEdges">Dictionary of flow assignments for edges</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="augmentingPathsFound">Number of augmenting paths found</param>
    /// <param name="iterations">Number of iterations performed</param>
    public FlowResult(T source, T sink, TCapacity maxFlow, 
        Dictionary<WeightedEdge<T, TCapacity>, TCapacity> flowEdges, 
        string algorithm, int augmentingPathsFound, int iterations)
    {
        Source = source;
        Sink = sink;
        MaxFlow = maxFlow;
        FlowEdges = new ReadOnlyDictionary<WeightedEdge<T, TCapacity>, TCapacity>(flowEdges);
        Algorithm = algorithm;
        AugmentingPathsFound = augmentingPathsFound;
        Iterations = iterations;
    }

    /// <summary>
    /// Gets the flow value for a specific edge.
    /// </summary>
    /// <param name="source">The source node of the edge</param>
    /// <param name="target">The target node of the edge</param>
    /// <returns>The flow value on the edge, or 0 if no flow exists</returns>
    public TCapacity GetFlow(T source, T target)
    {
        var edge = FlowEdges.Keys.FirstOrDefault(e => e.Source.Equals(source) && e.Target.Equals(target));
        return edge != null ? FlowEdges[edge] : default(TCapacity);
    }

    /// <summary>
    /// Gets whether an edge is saturated (flow equals capacity).
    /// </summary>
    /// <param name="source">The source node of the edge</param>
    /// <param name="target">The target node of the edge</param>
    /// <param name="capacity">The capacity of the edge</param>
    /// <returns>True if the edge is saturated, false otherwise</returns>
    public bool IsSaturated(T source, T target, TCapacity capacity)
    {
        var flow = GetFlow(source, target);
        return flow.Equals(capacity);
    }

    /// <summary>
    /// Gets the residual capacity of an edge (capacity minus flow).
    /// </summary>
    /// <param name="source">The source node of the edge</param>
    /// <param name="target">The target node of the edge</param>
    /// <param name="capacity">The capacity of the edge</param>
    /// <returns>The residual capacity of the edge</returns>
    public TCapacity GetResidualCapacity(T source, T target, TCapacity capacity)
    {
        var flow = GetFlow(source, target);
        return Subtract(capacity, flow);
    }

    /// <summary>
    /// Returns a string representation of the flow result.
    /// </summary>
    /// <returns>A string showing the maximum flow and algorithm used</returns>
    public override string ToString()
    {
        return $"Maximum flow from {Source} to {Sink} using {Algorithm}: {MaxFlow} " +
               $"({AugmentingPathsFound} augmenting paths, {Iterations} iterations)";
    }

    private static TCapacity Subtract(TCapacity a, TCapacity b)
    {
        if (typeof(TCapacity) == typeof(double)) return (TCapacity)(object)((double)(object)a - (double)(object)b);
        if (typeof(TCapacity) == typeof(int)) return (TCapacity)(object)((int)(object)a - (int)(object)b);
        if (typeof(TCapacity) == typeof(float)) return (TCapacity)(object)((float)(object)a - (float)(object)b);
        if (typeof(TCapacity) == typeof(decimal)) return (TCapacity)(object)((decimal)(object)a - (decimal)(object)b);
        
        return (TCapacity)(object)((dynamic)a - (dynamic)b);
    }
}

/// <summary>
/// Represents a minimum cut in a flow network.
/// Contains the two partitions of nodes and the total capacity of edges crossing the cut.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TCapacity">The type of edge capacities</typeparam>
public sealed class MinimumCut<T, TCapacity>
    where T : notnull
    where TCapacity : struct, IComparable<TCapacity>, IEquatable<TCapacity>, IConvertible
{
    /// <summary>
    /// Gets the source side of the cut (nodes reachable from the source in the residual graph).
    /// </summary>
    public IReadOnlySet<T> SourceSide { get; }

    /// <summary>
    /// Gets the sink side of the cut (nodes not reachable from the source in the residual graph).
    /// </summary>
    public IReadOnlySet<T> SinkSide { get; }

    /// <summary>
    /// Gets the total capacity of edges crossing from source side to sink side.
    /// </summary>
    public TCapacity CutCapacity { get; }

    /// <summary>
    /// Gets the edges crossing the cut.
    /// </summary>
    public IReadOnlyList<WeightedEdge<T, TCapacity>> CutEdges { get; }

    /// <summary>
    /// Initializes a new instance of the MinimumCut class.
    /// </summary>
    /// <param name="sourceSide">Nodes on the source side of the cut</param>
    /// <param name="sinkSide">Nodes on the sink side of the cut</param>
    /// <param name="cutCapacity">Total capacity of the cut</param>
    /// <param name="cutEdges">Edges crossing the cut</param>
    public MinimumCut(ISet<T> sourceSide, ISet<T> sinkSide, TCapacity cutCapacity, 
        IEnumerable<WeightedEdge<T, TCapacity>> cutEdges)
    {
        SourceSide = new ReadOnlySet<T>(sourceSide);
        SinkSide = new ReadOnlySet<T>(sinkSide);
        CutCapacity = cutCapacity;
        CutEdges = new ReadOnlyCollection<WeightedEdge<T, TCapacity>>(cutEdges.ToList());
    }

    /// <summary>
    /// Returns a string representation of the minimum cut.
    /// </summary>
    /// <returns>A string showing the cut capacity and partition sizes</returns>
    public override string ToString()
    {
        return $"Minimum cut: capacity {CutCapacity}, " +
               $"{SourceSide.Count} nodes on source side, {SinkSide.Count} nodes on sink side";
    }
}

/// <summary>
/// Read-only set implementation for MinimumCut.
/// </summary>
/// <typeparam name="T">The type of elements in the set</typeparam>
internal class ReadOnlySet<T> : IReadOnlySet<T>
{
    private readonly ISet<T> _innerSet;

    public ReadOnlySet(ISet<T> innerSet)
    {
        _innerSet = innerSet;
    }

    public int Count => _innerSet.Count;
    public bool Contains(T item) => _innerSet.Contains(item);
    public IEnumerator<T> GetEnumerator() => _innerSet.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    public bool IsProperSubsetOf(IEnumerable<T> other) => _innerSet.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => _innerSet.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => _innerSet.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => _innerSet.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => _innerSet.Overlaps(other);
    public bool SetEquals(IEnumerable<T> other) => _innerSet.SetEquals(other);
}
