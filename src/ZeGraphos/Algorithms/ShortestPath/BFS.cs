using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.ShortestPath;

/// <summary>
/// Implementation of Breadth-First Search for finding shortest paths in unweighted graphs.
/// Finds the shortest path (fewest edges) from a source node to all other nodes.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public static class BFS<T>
    where T : notnull
{
    /// <summary>
    /// Finds the shortest path from source to target using BFS.
    /// </summary>
    /// <param name="graph">The graph to search</param>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result with distance measured in number of edges</returns>
    public static PathResult<T, int> FindShortestPath(IGraph<T> graph, T source, T target)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");
        if (!graph.ContainsNode(target))
            throw new KeyNotFoundException($"Target node {target} not found in graph.");

        var distances = new Dictionary<T, int>();
        var previous = new Dictionary<T, T>();
        var visited = new HashSet<T>();
        var queue = new Queue<T>();
        var nodesVisited = 0;

        // Initialize
        distances[source] = 0;
        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            nodesVisited++;

            if (current.Equals(target))
                break;

            // Visit all neighbors
            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    distances[neighbor] = distances[current] + 1;
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Reconstruct path
        if (distances.ContainsKey(target))
        {
            var path = ReconstructPath(previous, source, target);
            return new PathResult<T, int>(source, target, distances[target], path, "BFS", nodesVisited);
        }
        else
        {
            return new PathResult<T, int>(source, target, "BFS", nodesVisited);
        }
    }

    /// <summary>
    /// Finds shortest paths from source to all reachable nodes.
    /// </summary>
    /// <param name="graph">The graph to search</param>
    /// <param name="source">The source node</param>
    /// <returns>All paths result containing distances and paths to all reachable nodes</returns>
    public static AllPathsResult<T, int> FindAllShortestPaths(IGraph<T> graph, T source)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");

        var distances = new Dictionary<T, int>();
        var previous = new Dictionary<T, T>();
        var visited = new HashSet<T>();
        var queue = new Queue<T>();
        var nodesVisited = 0;

        // Initialize
        distances[source] = 0;
        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            nodesVisited++;

            // Visit all neighbors
            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    distances[neighbor] = distances[current] + 1;
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Reconstruct all paths
        var paths = new Dictionary<T, List<T>>();
        foreach (var node in distances.Keys)
        {
            paths[node] = ReconstructPath(previous, source, node).ToList();
        }

        return new AllPathsResult<T, int>(source, distances, paths, "BFS", nodesVisited);
    }

    /// <summary>
    /// Performs a BFS traversal and returns the nodes in visitation order.
    /// </summary>
    /// <param name="graph">The graph to traverse</param>
    /// <param name="source">The starting node</param>
    /// <returns>Nodes in BFS visitation order</returns>
    public static IReadOnlyList<T> Traverse(IGraph<T> graph, T source)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");

        var visited = new HashSet<T>();
        var queue = new Queue<T>();
        var visitationOrder = new List<T>();

        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visitationOrder.Add(current);

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return new ReadOnlyCollection<T>(visitationOrder);
    }

    /// <summary>
    /// Checks if two nodes are connected by finding if a path exists between them.
    /// </summary>
    /// <param name="graph">The graph to check</param>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>True if the nodes are connected, false otherwise</returns>
    public static bool AreConnected(IGraph<T> graph, T source, T target)
    {
        if (!graph.ContainsNode(source) || !graph.ContainsNode(target))
            return false;

        if (source.Equals(target))
            return true;

        var visited = new HashSet<T>();
        var queue = new Queue<T>();

        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (neighbor.Equals(target))
                    return true;

                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Finds all connected components in the graph using BFS.
    /// </summary>
    /// <param name="graph">The graph to analyze</param>
    /// <returns>List of connected components, each being a list of nodes</returns>
    public static IReadOnlyList<IReadOnlyList<T>> FindConnectedComponents(IGraph<T> graph)
    {
        var visited = new HashSet<T>();
        var components = new List<List<T>>();

        foreach (var node in graph.GetNodes())
        {
            if (!visited.Contains(node))
            {
                var component = new List<T>();
                var queue = new Queue<T>();

                queue.Enqueue(node);
                visited.Add(node);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);

                    foreach (var neighbor in graph.GetNeighbors(current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                components.Add(component);
            }
        }

        return new ReadOnlyCollection<IReadOnlyList<T>>(components.Select(c => (IReadOnlyList<T>)new ReadOnlyCollection<T>(c)).ToList());
    }

    private static List<T> ReconstructPath(Dictionary<T, T> previous, T source, T target)
    {
        var path = new List<T>();
        var current = target;

        while (previous.ContainsKey(current))
        {
            path.Add(current);
            current = previous[current];
        }

        if (current.Equals(source))
        {
            path.Add(source);
            path.Reverse();
            return path;
        }

        return new List<T>(); // No path found
    }
}
