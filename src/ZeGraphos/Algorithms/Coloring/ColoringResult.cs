using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Coloring;

/// <summary>
/// Represents the result of a graph coloring algorithm execution.
/// Contains color assignments for each node and algorithm metadata.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public sealed class ColoringResult<T>
    where T : notnull
{
    /// <summary>
    /// Gets the color assignments for each node.
    /// Key is the node, value is the assigned color.
    /// </summary>
    public IReadOnlyDictionary<T, int> ColorAssignments { get; }

    /// <summary>
    /// Gets the number of colors used in the coloring.
    /// </summary>
    public int ChromaticNumber { get; }

    /// <summary>
    /// Gets the algorithm used to compute the coloring.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    /// Gets the number of iterations performed during the algorithm execution.
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// Gets whether the coloring is valid (no adjacent nodes share the same color).
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Initializes a new instance of the ColoringResult class.
    /// </summary>
    /// <param name="colorAssignments">Dictionary of color assignments for nodes</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="iterations">Number of iterations performed</param>
    /// <param name="isValid">Whether the coloring is valid</param>
    public ColoringResult(Dictionary<T, int> colorAssignments, string algorithm, int iterations, bool isValid = true)
    {
        ColorAssignments = new ReadOnlyDictionary<T, int>(colorAssignments);
        Algorithm = algorithm;
        NodeCount = colorAssignments.Count;
        Iterations = iterations;
        IsValid = isValid;
        ChromaticNumber = colorAssignments.Values.Distinct().Count();
    }

    /// <summary>
    /// Gets the color assigned to a specific node.
    /// </summary>
    /// <param name="node">The node to get color for</param>
    /// <returns>The color assigned to the node</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the node is not in the coloring</exception>
    public int GetColor(T node)
    {
        if (!ColorAssignments.ContainsKey(node))
            throw new KeyNotFoundException($"Node {node} not found in coloring.");

        return ColorAssignments[node];
    }

    /// <summary>
    /// Gets all nodes with a specific color.
    /// </summary>
    /// <param name="color">The color to get nodes for</param>
    /// <returns>A collection of nodes with the specified color</returns>
    public IReadOnlyCollection<T> GetNodesWithColor(int color)
    {
        var nodes = ColorAssignments
            .Where(kvp => kvp.Value == color)
            .Select(kvp => kvp.Key)
            .ToList();

        return new ReadOnlyCollection<T>(nodes);
    }

    /// <summary>
    /// Gets the color groups (nodes grouped by color).
    /// </summary>
    /// <returns>A dictionary where keys are colors and values are collections of nodes</returns>
    public IReadOnlyDictionary<int, IReadOnlyCollection<T>> GetColorGroups()
    {
        var groups = ColorAssignments
            .GroupBy(kvp => kvp.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<T>)new ReadOnlyCollection<T>(g.Select(kvp => kvp.Key).ToList()));

        return new ReadOnlyDictionary<int, IReadOnlyCollection<T>>(groups);
    }

    /// <summary>
    /// Checks if two nodes have the same color.
    /// </summary>
    /// <param name="node1">The first node</param>
    /// <param name="node2">The second node</param>
    /// <returns>True if both nodes have the same color, false otherwise</returns>
    public bool HaveSameColor(T node1, T node2)
    {
        if (!ColorAssignments.ContainsKey(node1) || !ColorAssignments.ContainsKey(node2))
            return false;

        return ColorAssignments[node1].Equals(ColorAssignments[node2]);
    }

    /// <summary>
    /// Returns a string representation of the coloring result.
    /// </summary>
    /// <returns>A string showing the chromatic number and algorithm used</returns>
    public override string ToString()
    {
        return $"Graph coloring using {Algorithm}: {ChromaticNumber} colors, " +
               $"{NodeCount} nodes ({Iterations} iterations, Valid: {IsValid})";
    }
}

/// <summary>
/// Represents different ordering strategies for graph coloring algorithms.
/// </summary>
public enum ColoringOrdering
{
    /// <summary>
    /// Natural order (as nodes appear in the graph)
    /// </summary>
    Natural,

    /// <summary>
    /// Order by degree (highest degree first)
    /// </summary>
    DegreeDescending,

    /// <summary>
    /// Order by degree (lowest degree first)
    /// </summary>
    DegreeAscending,

    /// <summary>
    /// Largest degree first (Welsh-Powell ordering)
    /// </summary>
    LargestDegreeFirst,

    /// <summary>
    /// Smallest degree last
    /// </summary>
    SmallestDegreeLast,

    /// <summary>
    /// Random order
    /// </summary>
    Random
}

/// <summary>
/// Represents different coloring strategies for greedy algorithms.
/// </summary>
public enum ColoringStrategy
{
    /// <summary>
    /// Use the smallest available color
    /// </summary>
    FirstAvailable,

    /// <summary>
    /// Use the color that minimizes conflicts with uncolored neighbors
    /// </summary>
    LeastConflict,

    /// <summary>
    /// Use the color that maximizes distance from already used colors
    /// </summary>
    MaximumDistance
}
