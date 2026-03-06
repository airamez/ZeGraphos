namespace ZeGraphos.Core.Common;

/// <summary>
/// Represents a node in a graph with an identifier and optional data.
/// </summary>
/// <typeparam name="T">The type of the node identifier/data</typeparam>
public sealed record Node<T>(T Id)
{
    /// <summary>
    /// Gets the identifier of the node.
    /// </summary>
    public T Id { get; } = Id;

    /// <summary>
    /// Returns a string representation of the node.
    /// </summary>
    /// <returns>The node identifier as a string</returns>
    public override string ToString() => Id?.ToString() ?? "null";

    /// <summary>
    /// Implicitly converts a node to its identifier.
    /// </summary>
    /// <param name="node">The node to convert</param>
    /// <returns>The node identifier</returns>
    public static implicit operator T(Node<T> node) => node.Id;

    /// <summary>
    /// Implicitly converts an identifier to a node.
    /// </summary>
    /// <param name="id">The identifier to convert</param>
    /// <returns>A new node with the given identifier</returns>
    public static implicit operator Node<T>(T id) => new(id);
}
