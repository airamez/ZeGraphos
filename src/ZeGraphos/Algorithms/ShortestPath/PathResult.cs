using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ZeGraphos.Algorithms.ShortestPath;

/// <summary>
/// Represents the result of a shortest path algorithm execution.
/// Contains the path, distance, and algorithm metadata.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TDistance">The type of distance values</typeparam>
public sealed class PathResult<T, TDistance>
    where T : notnull
    where TDistance : struct, IComparable<TDistance>, IEquatable<TDistance>, IConvertible
{
    /// <summary>
    /// Gets the source node of the path.
    /// </summary>
    public T Source { get; }

    /// <summary>
    /// Gets the target node of the path.
    /// </summary>
    public T Target { get; }

    /// <summary>
    /// Gets the total distance of the path.
    /// </summary>
    public TDistance Distance { get; }

    /// <summary>
    /// Gets the sequence of nodes in the path from source to target.
    /// </summary>
    public IReadOnlyList<T> Path { get; }

    /// <summary>
    /// Gets whether a path was found between the source and target.
    /// </summary>
    public bool PathFound { get; }

    /// <summary>
    /// Gets the algorithm used to compute the path.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of nodes visited during the algorithm execution.
    /// </summary>
    public int NodesVisited { get; }

    /// <summary>
    /// Initializes a new instance of the PathResult class for a successful path.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="distance">The total distance</param>
    /// <param name="path">The sequence of nodes in the path</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="nodesVisited">Number of nodes visited</param>
    public PathResult(T source, T target, TDistance distance, IEnumerable<T> path, string algorithm, int nodesVisited)
    {
        Source = source;
        Target = target;
        Distance = distance;
        Path = new ReadOnlyCollection<T>(new List<T>(path));
        PathFound = true;
        Algorithm = algorithm;
        NodesVisited = nodesVisited;
    }

    /// <summary>
    /// Initializes a new instance of the PathResult class for a failed path search.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="nodesVisited">Number of nodes visited</param>
    public PathResult(T source, T target, string algorithm, int nodesVisited)
    {
        Source = source;
        Target = target;
        Distance = default!;
        Path = new ReadOnlyCollection<T>(new List<T>());
        PathFound = false;
        Algorithm = algorithm;
        NodesVisited = nodesVisited;
    }

    /// <summary>
    /// Returns a string representation of the path result.
    /// </summary>
    /// <returns>A string showing the path and distance</returns>
    public override string ToString()
    {
        if (!PathFound)
            return $"No path found from {Source} to {Target} using {Algorithm}";

        var pathStr = string.Join(" -> ", Path);
        return $"Path from {Source} to {Target} using {Algorithm}: {pathStr} (Distance: {Distance})";
    }
}

/// <summary>
/// Represents the result of an all-pairs shortest path algorithm.
/// Contains shortest paths from a source node to all other reachable nodes.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TDistance">The type of distance values</typeparam>
public sealed class AllPathsResult<T, TDistance>
    where T : notnull
    where TDistance : struct, IComparable<TDistance>, IEquatable<TDistance>, IConvertible
{
    /// <summary>
    /// Gets the source node for all paths.
    /// </summary>
    public T Source { get; }

    /// <summary>
    /// Gets the distances from the source to all reachable nodes.
    /// </summary>
    public IReadOnlyDictionary<T, TDistance> Distances { get; }

    /// <summary>
    /// Gets the paths from the source to all reachable nodes.
    /// </summary>
    public IReadOnlyDictionary<T, IReadOnlyList<T>> Paths { get; }

    /// <summary>
    /// Gets the algorithm used to compute the paths.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of nodes visited during the algorithm execution.
    /// </summary>
    public int NodesVisited { get; }

    /// <summary>
    /// Initializes a new instance of the AllPathsResult class.
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="distances">Dictionary of distances to all nodes</param>
    /// <param name="paths">Dictionary of paths to all nodes</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="nodesVisited">Number of nodes visited</param>
    public AllPathsResult(T source, Dictionary<T, TDistance> distances, Dictionary<T, List<T>> paths, string algorithm, int nodesVisited)
    {
        Source = source;
        Distances = new ReadOnlyDictionary<T, TDistance>(distances);
        Paths = new ReadOnlyDictionary<T, IReadOnlyList<T>>(paths.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<T>)new ReadOnlyCollection<T>(kvp.Value)));
        Algorithm = algorithm;
        NodesVisited = nodesVisited;
    }

    /// <summary>
    /// Gets the shortest path to a specific target node.
    /// </summary>
    /// <param name="target">The target node</param>
    /// <returns>The path result for the target, or null if unreachable</returns>
    public PathResult<T, TDistance>? GetPathTo(T target)
    {
        if (!Distances.ContainsKey(target))
            return null;

        return new PathResult<T, TDistance>(Source, target, Distances[target], Paths[target], Algorithm, NodesVisited);
    }

    /// <summary>
    /// Gets whether a target node is reachable from the source.
    /// </summary>
    /// <param name="target">The target node</param>
    /// <returns>True if reachable, false otherwise</returns>
    public bool IsReachable(T target) => Distances.ContainsKey(target);
}
