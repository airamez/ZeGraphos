namespace ZeGraphos.Core.Common;

/// <summary>
/// Represents an edge in a graph connecting a source node to a target node.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public record Edge<T>(T Source, T Target)
{
    /// <summary>
    /// Gets the source node of the edge.
    /// </summary>
    public T Source { get; } = Source;

    /// <summary>
    /// Gets the target node of the edge.
    /// </summary>
    public T Target { get; } = Target;

    /// <summary>
    /// Gets whether this edge is a self-loop (source equals target).
    /// </summary>
    public bool IsSelfLoop => EqualityComparer<T>.Default.Equals(Source, Target);

    /// <summary>
    /// Returns a string representation of the edge.
    /// </summary>
    /// <returns>A string in the format "Source -> Target"</returns>
    public override string ToString() => $"{Source} -> {Target}";

    /// <summary>
    /// Creates a reversed edge (target becomes source, source becomes target).
    /// </summary>
    /// <returns>A new edge with reversed direction</returns>
    public Edge<T> Reverse() => new(Target, Source);
}

/// <summary>
/// Represents a weighted edge in a graph with a specific weight value.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of the edge weight</typeparam>
public record WeightedEdge<T, TWeight>(T Source, T Target, TWeight Weight) : Edge<T>(Source, Target)
{
    /// <summary>
    /// Gets the weight of the edge.
    /// </summary>
    public TWeight Weight { get; } = Weight;

    /// <summary>
    /// Returns a string representation of the weighted edge.
    /// </summary>
    /// <returns>A string in the format "Source -> Target (Weight)"</returns>
    public override string ToString() => $"{Source} -> {Target} ({Weight})";

    /// <summary>
    /// Creates a reversed weighted edge with the same weight.
    /// </summary>
    /// <returns>A new weighted edge with reversed direction</returns>
    public new WeightedEdge<T, TWeight> Reverse() => new(Target, Source, Weight);
}
