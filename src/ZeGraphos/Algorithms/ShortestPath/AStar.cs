using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.ShortestPath;

/// <summary>
/// Implementation of A* search algorithm for finding shortest paths with heuristic guidance.
/// Combines Dijkstra's algorithm with a heuristic function to guide the search towards the target.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public static class AStar<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Represents a heuristic function that estimates the cost from a node to the target.
    /// </summary>
    /// <param name="current">The current node</param>
    /// <param name="target">The target node</param>
    /// <returns>Estimated cost from current to target</returns>
    public delegate TWeight HeuristicFunction(T current, T target);

    /// <summary>
    /// Finds the shortest path from source to target using A* algorithm.
    /// </summary>
    /// <param name="graph">The weighted graph to search</param>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <param name="heuristic">The heuristic function to guide the search</param>
    /// <returns>The shortest path result</returns>
    public static PathResult<T, TWeight> FindShortestPath(IWeightedGraph<T, TWeight> graph, T source, T target, HeuristicFunction heuristic)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");
        if (!graph.ContainsNode(target))
            throw new KeyNotFoundException($"Target node {target} not found in graph.");

        var gScore = new Dictionary<T, TWeight>(); // Cost from start to current
        var fScore = new Dictionary<T, TWeight>(); // Estimated total cost (g + h)
        var previous = new Dictionary<T, T>();
        var openSet = new PriorityQueue<T, TWeight>();
        var closedSet = new HashSet<T>();
        var nodesVisited = 0;

        // Initialize scores
        foreach (var node in graph.GetNodes())
        {
            gScore[node] = GetMaxValue();
            fScore[node] = GetMaxValue();
        }
        gScore[source] = GetMinValue();
        fScore[source] = heuristic(source, target);

        openSet.Enqueue(source, fScore[source]);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            nodesVisited++;

            if (current.Equals(target))
            {
                var path = ReconstructPath(previous, source, target);
                return new PathResult<T, TWeight>(source, target, gScore[target], path, "A*", nodesVisited);
            }

            closedSet.Add(current);

            // Examine neighbors
            foreach (var edge in graph.GetWeightedNeighbors(current))
            {
                var neighbor = edge.Target;
                var weight = edge.Weight;

                if (closedSet.Contains(neighbor))
                    continue;

                var tentativeGScore = Add(gScore[current], weight);

                if (!gScore.ContainsKey(neighbor) || Compare(tentativeGScore, gScore[neighbor]) < 0)
                {
                    previous[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = Add(tentativeGScore, heuristic(neighbor, target));

                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        // No path found
        return new PathResult<T, TWeight>(source, target, "A*", nodesVisited);
    }

    /// <summary>
    /// Creates a Manhattan distance heuristic for 2D grid coordinates.
    /// </summary>
    /// <param name="coordinates">Dictionary mapping nodes to (x, y) coordinates</param>
    /// <returns>A heuristic function using Manhattan distance</returns>
    public static HeuristicFunction CreateManhattanHeuristic(Dictionary<T, (int x, int y)> coordinates)
    {
        return (current, target) =>
        {
            var (x1, y1) = coordinates[current];
            var (x2, y2) = coordinates[target];
            var distance = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
            
            if (typeof(TWeight) == typeof(double)) return (TWeight)(object)(double)distance;
            if (typeof(TWeight) == typeof(int)) return (TWeight)(object)distance;
            if (typeof(TWeight) == typeof(float)) return (TWeight)(object)(float)distance;
            if (typeof(TWeight) == typeof(decimal)) return (TWeight)(object)(decimal)distance;
            
            return (TWeight)(object)distance;
        };
    }

    /// <summary>
    /// Creates an Euclidean distance heuristic for 2D grid coordinates.
    /// </summary>
    /// <param name="coordinates">Dictionary mapping nodes to (x, y) coordinates</param>
    /// <returns>A heuristic function using Euclidean distance</returns>
    public static HeuristicFunction CreateEuclideanHeuristic(Dictionary<T, (double x, double y)> coordinates)
    {
        return (current, target) =>
        {
            var (x1, y1) = coordinates[current];
            var (x2, y2) = coordinates[target];
            var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            
            if (typeof(TWeight) == typeof(double)) return (TWeight)(object)distance;
            if (typeof(TWeight) == typeof(float)) return (TWeight)(object)(float)distance;
            if (typeof(TWeight) == typeof(decimal)) return (TWeight)(object)(decimal)distance;
            
            return (TWeight)(object)distance;
        };
    }

    /// <summary>
    /// Creates a zero heuristic (equivalent to Dijkstra's algorithm).
    /// </summary>
    /// <returns>A heuristic function that always returns zero</returns>
    public static HeuristicFunction CreateZeroHeuristic()
    {
        return (_, _) => GetMinValue();
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

    private static TWeight GetMinValue()
    {
        if (typeof(TWeight) == typeof(double)) return (TWeight)(object)0.0;
        if (typeof(TWeight) == typeof(int)) return (TWeight)(object)0;
        if (typeof(TWeight) == typeof(float)) return (TWeight)(object)0.0f;
        if (typeof(TWeight) == typeof(decimal)) return (TWeight)(object)0m;
        
        return default!;
    }

    private static TWeight GetMaxValue()
    {
        if (typeof(TWeight) == typeof(double)) return (TWeight)(object)double.MaxValue;
        if (typeof(TWeight) == typeof(int)) return (TWeight)(object)int.MaxValue;
        if (typeof(TWeight) == typeof(float)) return (TWeight)(object)float.MaxValue;
        if (typeof(TWeight) == typeof(decimal)) return (TWeight)(object)decimal.MaxValue;
        
        dynamic maxValue = default(TWeight);
        if (typeof(TWeight).IsEnum)
        {
            var values = Enum.GetValues(typeof(TWeight));
            maxValue = values.GetValue(values.Length - 1) ?? maxValue;
        }
        return maxValue;
    }

    private static TWeight Add(TWeight a, TWeight b)
    {
        if (typeof(TWeight) == typeof(double)) return (TWeight)(object)((double)(object)a + (double)(object)b);
        if (typeof(TWeight) == typeof(int)) return (TWeight)(object)((int)(object)a + (int)(object)b);
        if (typeof(TWeight) == typeof(float)) return (TWeight)(object)((float)(object)a + (float)(object)b);
        if (typeof(TWeight) == typeof(decimal)) return (TWeight)(object)((decimal)(object)a + (decimal)(object)b);
        
        return (TWeight)(object)((dynamic)a + (dynamic)b);
    }

    private static int Compare(TWeight a, TWeight b)
    {
        if (typeof(TWeight) == typeof(double)) return ((double)(object)a).CompareTo((double)(object)b);
        if (typeof(TWeight) == typeof(int)) return ((int)(object)a).CompareTo((int)(object)b);
        if (typeof(TWeight) == typeof(float)) return ((float)(object)a).CompareTo((float)(object)b);
        if (typeof(TWeight) == typeof(decimal)) return ((decimal)(object)a).CompareTo((decimal)(object)b);
        
        return ((IComparable<TWeight>)a).CompareTo(b);
    }
}
