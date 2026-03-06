using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.TopologicalSort;

/// <summary>
/// Implementation of Kahn's algorithm for topological sorting using a queue.
/// Processes nodes by repeatedly removing nodes with no incoming edges.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public static class Kahn<T>
    where T : notnull
{
    /// <summary>
    /// Performs a topological sort using Kahn's algorithm.
    /// </summary>
    /// <param name="graph">The directed graph to sort</param>
    /// <returns>The topological sort result</returns>
    /// <exception cref="ArgumentException">Thrown when the graph is not directed</exception>
    public static TopologicalSortResult<T> Sort(IGraph<T> graph)
    {
        if (!graph.IsDirected)
            throw new ArgumentException("Kahn's algorithm requires a directed graph.");

        if (graph.NodeCount == 0)
            return new TopologicalSortResult<T>(Array.Empty<T>(), "Kahn", 0);

        var inDegree = new Dictionary<T, int>();
        var queue = new Queue<T>();
        var sortedOrder = new List<T>();
        var iterations = 0;

        // Step 1: Calculate in-degrees
        foreach (var node in graph.GetNodes())
        {
            inDegree[node] = 0;
        }

        foreach (var edge in graph.GetEdges())
        {
            inDegree[edge.Target]++;
        }

        // Step 2: Find nodes with no incoming edges
        foreach (var node in graph.GetNodes())
        {
            if (inDegree[node] == 0)
            {
                queue.Enqueue(node);
            }
        }

        // Step 3: Process nodes
        while (queue.Count > 0)
        {
            iterations++;

            var current = queue.Dequeue();
            sortedOrder.Add(current);

            // Remove edges from current node and update in-degrees
            foreach (var neighbor in graph.GetNeighbors(current))
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Step 4: Check for cycles
        if (sortedOrder.Count == graph.NodeCount)
        {
            return new TopologicalSortResult<T>(sortedOrder, "Kahn", iterations);
        }
        else
        {
            var unsortedNodes = graph.GetNodes().Where(n => !sortedOrder.Contains(n)).ToList();
            return new TopologicalSortResult<T>(sortedOrder, unsortedNodes, "Kahn", iterations);
        }
    }

    /// <summary>
    /// Performs a topological sort using Kahn's algorithm with priority queue for deterministic ordering.
    /// </summary>
    /// <param name="graph">The directed graph to sort</param>
    /// <param name="comparer">Optional comparer for node ordering</param>
    /// <returns>The topological sort result</returns>
    public static TopologicalSortResult<T> SortDeterministic(IGraph<T> graph, IComparer<T>? comparer = null)
    {
        if (!graph.IsDirected)
            throw new ArgumentException("Kahn's algorithm requires a directed graph.");

        if (graph.NodeCount == 0)
            return new TopologicalSortResult<T>(Array.Empty<T>(), "Kahn (Deterministic)", 0);

        var inDegree = new Dictionary<T, int>();
        var priorityQueue = new PriorityQueue<T>(comparer ?? Comparer<T>.Default);
        var sortedOrder = new List<T>();
        var iterations = 0;

        // Step 1: Calculate in-degrees
        foreach (var node in graph.GetNodes())
        {
            inDegree[node] = 0;
        }

        foreach (var edge in graph.GetEdges())
        {
            inDegree[edge.Target]++;
        }

        // Step 2: Find nodes with no incoming edges
        foreach (var node in graph.GetNodes())
        {
            if (inDegree[node] == 0)
            {
                priorityQueue.Enqueue(node);
            }
        }

        // Step 3: Process nodes
        while (priorityQueue.Count > 0)
        {
            iterations++;

            var current = priorityQueue.Dequeue();
            sortedOrder.Add(current);

            // Remove edges from current node and update in-degrees
            foreach (var neighbor in graph.GetNeighbors(current))
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    priorityQueue.Enqueue(neighbor);
                }
            }
        }

        // Step 4: Check for cycles
        if (sortedOrder.Count == graph.NodeCount)
        {
            return new TopologicalSortResult<T>(sortedOrder, "Kahn (Deterministic)", iterations);
        }
        else
        {
            var unsortedNodes = graph.GetNodes().Where(n => !sortedOrder.Contains(n)).ToList();
            return new TopologicalSortResult<T>(sortedOrder, unsortedNodes, "Kahn (Deterministic)", iterations);
        }
    }

    /// <summary>
    /// Detects if the graph contains cycles using Kahn's algorithm.
    /// </summary>
    /// <param name="graph">The directed graph to check</param>
    /// <returns>True if the graph contains cycles, false otherwise</returns>
    public static bool HasCycle(IGraph<T> graph)
    {
        if (!graph.IsDirected)
            return false; // Undirected graphs are not considered for cycle detection in this context

        var result = Sort(graph);
        return !result.IsDAG;
    }

    /// <summary>
    /// Finds all source nodes (nodes with no incoming edges) in the graph.
    /// </summary>
    /// <param name="graph">The directed graph</param>
    /// <returns>A collection of source nodes</returns>
    public static IReadOnlyCollection<T> FindSourceNodes(IGraph<T> graph)
    {
        if (!graph.IsDirected)
            throw new ArgumentException("Source node detection requires a directed graph.");

        var inDegree = new Dictionary<T, int>();

        // Calculate in-degrees
        foreach (var node in graph.GetNodes())
        {
            inDegree[node] = 0;
        }

        foreach (var edge in graph.GetEdges())
        {
            inDegree[edge.Target]++;
        }

        // Find nodes with no incoming edges
        var sourceNodes = graph.GetNodes().Where(n => inDegree[n] == 0).ToList();
        return new ReadOnlyCollection<T>(sourceNodes);
    }

    /// <summary>
    /// Finds all sink nodes (nodes with no outgoing edges) in the graph.
    /// </summary>
    /// <param name="graph">The directed graph</param>
    /// <returns>A collection of sink nodes</returns>
    public static IReadOnlyCollection<T> FindSinkNodes(IGraph<T> graph)
    {
        if (!graph.IsDirected)
            throw new ArgumentException("Sink node detection requires a directed graph.");

        var sinkNodes = graph.GetNodes().Where(n => graph.GetDegree(n) == 0).ToList();
        return new ReadOnlyCollection<T>(sinkNodes);
    }

    /// <summary>
    /// Priority queue implementation for deterministic topological sorting.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the queue</typeparam>
    private class PriorityQueue<TItem>
    {
        private readonly List<TItem> _elements = new();
        private readonly IComparer<TItem> _comparer;

        public PriorityQueue(IComparer<TItem> comparer)
        {
            _comparer = comparer;
        }

        public int Count => _elements.Count;

        public void Enqueue(TItem item)
        {
            _elements.Add(item);
            var childIndex = _elements.Count - 1;
            while (childIndex > 0)
            {
                var parentIndex = (childIndex - 1) / 2;
                if (_comparer.Compare(_elements[childIndex], _elements[parentIndex]) >= 0)
                    break;

                var temp = _elements[childIndex];
                _elements[childIndex] = _elements[parentIndex];
                _elements[parentIndex] = temp;
                childIndex = parentIndex;
            }
        }

        public TItem Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var result = _elements[0];
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
                if (rightChildIndex <= lastIndex && _comparer.Compare(_elements[rightChildIndex], _elements[leftChildIndex]) < 0)
                    leftChildIndex = rightChildIndex;

                if (_comparer.Compare(_elements[parentIndex], _elements[leftChildIndex]) <= 0)
                    break;

                var temp = _elements[parentIndex];
                _elements[parentIndex] = _elements[leftChildIndex];
                _elements[leftChildIndex] = temp;
                parentIndex = leftChildIndex;
            }

            return result;
        }
    }
}
