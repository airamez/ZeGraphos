using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.ShortestPath;

/// <summary>
/// Implementation of Dijkstra's shortest path algorithm using a priority queue.
/// Finds the shortest path from a source node to all other nodes in a weighted graph.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public static class Dijkstra<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Finds the shortest path from source to target using Dijkstra's algorithm.
    /// </summary>
    /// <param name="graph">The weighted graph to search</param>
    /// <param name="source">The source node</param>
    /// <param name="target">The target node</param>
    /// <returns>The shortest path result</returns>
    public static PathResult<T, TWeight> FindShortestPath(IWeightedGraph<T, TWeight> graph, T source, T target)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");
        if (!graph.ContainsNode(target))
            throw new KeyNotFoundException($"Target node {target} not found in graph.");

        var distances = new Dictionary<T, TWeight>();
        var previous = new Dictionary<T, T>();
        var visited = new HashSet<T>();
        var priorityQueue = new PriorityQueue<T, TWeight>();
        var nodesVisited = 0;

        // Initialize distances
        foreach (var node in graph.GetNodes())
        {
            distances[node] = GetMaxValue();
        }
        distances[source] = GetMinValue();

        // Initialize priority queue
        priorityQueue.Enqueue(source, GetMinValue());

        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();
            nodesVisited++;

            if (current.Equals(target))
                break;

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // Relax edges
            foreach (var edge in graph.GetWeightedNeighbors(current))
            {
                var neighbor = edge.Target;
                var weight = edge.Weight;

                if (visited.Contains(neighbor))
                    continue;

                var newDistance = Add(distances[current], weight);
                if (Compare(newDistance, distances[neighbor]) < 0)
                {
                    distances[neighbor] = newDistance;
                    previous[neighbor] = current;
                    priorityQueue.Enqueue(neighbor, newDistance);
                }
            }
        }

        // Reconstruct path
        if (distances.ContainsKey(target) && !distances[target].Equals(GetMaxValue()))
        {
            var path = ReconstructPath(previous, source, target);
            return new PathResult<T, TWeight>(source, target, distances[target], path, "Dijkstra", nodesVisited);
        }
        else
        {
            return new PathResult<T, TWeight>(source, target, "Dijkstra", nodesVisited);
        }
    }

    /// <summary>
    /// Finds shortest paths from source to all reachable nodes.
    /// </summary>
    /// <param name="graph">The weighted graph to search</param>
    /// <param name="source">The source node</param>
    /// <returns>All paths result containing distances and paths to all reachable nodes</returns>
    public static AllPathsResult<T, TWeight> FindAllShortestPaths(IWeightedGraph<T, TWeight> graph, T source)
    {
        if (!graph.ContainsNode(source))
            throw new KeyNotFoundException($"Source node {source} not found in graph.");

        var distances = new Dictionary<T, TWeight>();
        var previous = new Dictionary<T, T>();
        var visited = new HashSet<T>();
        var priorityQueue = new PriorityQueue<T, TWeight>();
        var nodesVisited = 0;

        // Initialize distances
        foreach (var node in graph.GetNodes())
        {
            distances[node] = GetMaxValue();
        }
        distances[source] = GetMinValue();

        // Initialize priority queue
        priorityQueue.Enqueue(source, GetMinValue());

        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();
            nodesVisited++;

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // Relax edges
            foreach (var edge in graph.GetWeightedNeighbors(current))
            {
                var neighbor = edge.Target;
                var weight = edge.Weight;

                if (visited.Contains(neighbor))
                    continue;

                var newDistance = Add(distances[current], weight);
                if (Compare(newDistance, distances[neighbor]) < 0)
                {
                    distances[neighbor] = newDistance;
                    previous[neighbor] = current;
                    priorityQueue.Enqueue(neighbor, newDistance);
                }
            }
        }

        // Reconstruct all paths
        var paths = new Dictionary<T, List<T>>();
        foreach (var node in distances.Keys.Where(n => !distances[n].Equals(GetMaxValue())))
        {
            paths[node] = ReconstructPath(previous, source, node).ToList();
        }

        return new AllPathsResult<T, TWeight>(source, distances, paths, "Dijkstra", nodesVisited);
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
        
        // For other numeric types, use default
        return default!;
    }

    private static TWeight GetMaxValue()
    {
        if (typeof(TWeight) == typeof(double)) return (TWeight)(object)double.MaxValue;
        if (typeof(TWeight) == typeof(int)) return (TWeight)(object)int.MaxValue;
        if (typeof(TWeight) == typeof(float)) return (TWeight)(object)float.MaxValue;
        if (typeof(TWeight) == typeof(decimal)) return (TWeight)(object)decimal.MaxValue;
        
        // For other numeric types, use dynamic
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
        
        // For other numeric types, use dynamic
        return (TWeight)(object)((dynamic)a + (dynamic)b);
    }

    private static int Compare(TWeight a, TWeight b)
    {
        if (typeof(TWeight) == typeof(double)) return ((double)(object)a).CompareTo((double)(object)b);
        if (typeof(TWeight) == typeof(int)) return ((int)(object)a).CompareTo((int)(object)b);
        if (typeof(TWeight) == typeof(float)) return ((float)(object)a).CompareTo((float)(object)b);
        if (typeof(TWeight) == typeof(decimal)) return ((decimal)(object)a).CompareTo((decimal)(object)b);
        
        // For other types, use IComparable
        return ((IComparable<TWeight>)a).CompareTo(b);
    }
}

/// <summary>
/// Simple priority queue implementation for Dijkstra's algorithm.
/// </summary>
/// <typeparam name="T">The type of elements</typeparam>
/// <typeparam name="TPriority">The type of priority values</typeparam>
internal class PriorityQueue<T, TPriority>
    where TPriority : IComparable<TPriority>
{
    private readonly List<(T Item, TPriority Priority)> _elements = new();

    public int Count => _elements.Count;

    public void Enqueue(T item, TPriority priority)
    {
        _elements.Add((item, priority));
        var childIndex = _elements.Count - 1;
        while (childIndex > 0)
        {
            var parentIndex = (childIndex - 1) / 2;
            if (_elements[childIndex].Priority.CompareTo(_elements[parentIndex].Priority) >= 0)
                break;

            var temp = _elements[childIndex];
            _elements[childIndex] = _elements[parentIndex];
            _elements[parentIndex] = temp;
            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty");

        var result = _elements[0].Item;
        var lastIndex = _elements.Count - 1;
        _elements[0] = _elements[lastIndex];
        _elements.RemoveAt(lastIndex);

        lastIndex--;
        var parentIndex = 0;
        while (true)
        {
            var leftChildIndex = parentIndex * 2 + 1;
            if (leftChildIndex > lastIndex)
                break;

            var rightChildIndex = leftChildIndex + 1;
            if (rightChildIndex <= lastIndex && _elements[rightChildIndex].Priority.CompareTo(_elements[leftChildIndex].Priority) < 0)
                leftChildIndex = rightChildIndex;

            if (_elements[parentIndex].Priority.CompareTo(_elements[leftChildIndex].Priority) <= 0)
                break;

            var temp = _elements[parentIndex];
            _elements[parentIndex] = _elements[leftChildIndex];
            _elements[leftChildIndex] = temp;
            parentIndex = leftChildIndex;
        }

        return result;
    }
}
