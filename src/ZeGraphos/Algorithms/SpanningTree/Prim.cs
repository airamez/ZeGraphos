using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.SpanningTree;

/// <summary>
/// Implementation of Prim's algorithm using a priority queue.
/// Grows a minimum spanning tree from a starting node by always adding the cheapest edge.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public static class Prim<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Computes a minimum spanning tree using Prim's algorithm.
    /// </summary>
    /// <param name="graph">The weighted undirected graph</param>
    /// <param name="startNode">The starting node for the MST (optional, uses first node if null)</param>
    /// <returns>The minimum spanning tree result</returns>
    public static MSTResult<T, TWeight> ComputeMST(IWeightedGraph<T, TWeight> graph, T? startNode = default)
    {
        if (graph.IsDirected)
            throw new ArgumentException("Prim's algorithm requires an undirected graph.");

        if (graph.NodeCount == 0)
            throw new ArgumentException("Graph cannot be empty.");

        var nodes = graph.GetNodes().ToList();
        var start = startNode?.Equals(default) == true ? nodes[0] : startNode ?? nodes[0];

        if (!graph.ContainsNode(start))
            throw new KeyNotFoundException($"Start node {start} not found in graph.");

        var mstEdges = new List<WeightedEdge<T, TWeight>>();
        var visited = new HashSet<T>();
        var priorityQueue = new PriorityQueue();
        var iterations = 0;

        visited.Add(start);

        // Add all edges from start node to priority queue
        foreach (var edge in graph.GetWeightedNeighbors(start))
        {
            priorityQueue.Enqueue(edge);
        }

        while (priorityQueue.Count > 0 && mstEdges.Count < nodes.Count - 1)
        {
            iterations++;

            var edge = priorityQueue.Dequeue();

            // Find the unvisited endpoint of the edge
            T unvisitedNode;
            if (visited.Contains(edge.Source) && !visited.Contains(edge.Target))
            {
                unvisitedNode = edge.Target;
            }
            else if (!visited.Contains(edge.Source) && visited.Contains(edge.Target))
            {
                unvisitedNode = edge.Source;
            }
            else
            {
                continue; // Both endpoints visited or both unvisited, skip
            }

            // Add edge to MST
            mstEdges.Add(edge);
            visited.Add(unvisitedNode);

            // Add all edges from the newly visited node to priority queue
            foreach (var newEdge in graph.GetWeightedNeighbors(unvisitedNode))
            {
                if (!visited.Contains(newEdge.Target))
                {
                    priorityQueue.Enqueue(newEdge);
                }
            }
        }

        return new MSTResult<T, TWeight>(mstEdges, "Prim", graph.NodeCount, iterations);
    }

    /// <summary>
    /// Computes minimum spanning trees for all connected components in a disconnected graph.
    /// </summary>
    /// <param name="graph">The weighted undirected graph (may be disconnected)</param>
    /// <returns>The minimum spanning forest result</returns>
    public static ForestResult<T, TWeight> ComputeMinimumForest(IWeightedGraph<T, TWeight> graph)
    {
        if (graph.IsDirected)
            throw new ArgumentException("Prim's algorithm requires an undirected graph.");

        var trees = new List<MSTResult<T, TWeight>>();
        var visited = new HashSet<T>();
        var totalIterations = 0;

        foreach (var node in graph.GetNodes())
        {
            if (!visited.Contains(node))
            {
                // Find connected component using BFS
                var component = GetConnectedComponent(graph, node, visited);
                
                if (component.Count == 1)
                {
                    // Single node component
                    trees.Add(new MSTResult<T, TWeight>(Array.Empty<WeightedEdge<T, TWeight>>(), "Prim", 1, 0));
                }
                else
                {
                    // Create a subgraph for this component and compute MST
                    var componentMST = ComputeMSTForComponent(graph, component);
                    trees.Add(componentMST);
                    totalIterations += componentMST.Iterations;
                }
            }
        }

        return new ForestResult<T, TWeight>(trees, "Prim");
    }

    private static HashSet<T> GetConnectedComponent(IWeightedGraph<T, TWeight> graph, T startNode, HashSet<T> globalVisited)
    {
        var component = new HashSet<T>();
        var queue = new Queue<T>();

        queue.Enqueue(startNode);
        component.Add(startNode);
        globalVisited.Add(startNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!globalVisited.Contains(neighbor))
                {
                    globalVisited.Add(neighbor);
                    component.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return component;
    }

    private static MSTResult<T, TWeight> ComputeMSTForComponent(IWeightedGraph<T, TWeight> graph, HashSet<T> component)
    {
        var startNode = component.First();
        var mstEdges = new List<WeightedEdge<T, TWeight>>();
        var visited = new HashSet<T>();
        var priorityQueue = new PriorityQueue();
        var iterations = 0;

        visited.Add(startNode);

        // Add all edges from start node to priority queue
        foreach (var edge in graph.GetWeightedNeighbors(startNode))
        {
            if (component.Contains(edge.Target))
            {
                priorityQueue.Enqueue(edge);
            }
        }

        while (priorityQueue.Count > 0 && mstEdges.Count < component.Count - 1)
        {
            iterations++;

            var edge = priorityQueue.Dequeue();

            // Find the unvisited endpoint of the edge
            T unvisitedNode;
            if (visited.Contains(edge.Source) && !visited.Contains(edge.Target))
            {
                unvisitedNode = edge.Target;
            }
            else if (!visited.Contains(edge.Source) && visited.Contains(edge.Target))
            {
                unvisitedNode = edge.Source;
            }
            else
            {
                continue; // Both endpoints visited or both unvisited, skip
            }

            // Add edge to MST
            mstEdges.Add(edge);
            visited.Add(unvisitedNode);

            // Add all edges from the newly visited node to priority queue
            foreach (var newEdge in graph.GetWeightedNeighbors(unvisitedNode))
            {
                if (component.Contains(newEdge.Target) && !visited.Contains(newEdge.Target))
                {
                    priorityQueue.Enqueue(newEdge);
                }
            }
        }

        return new MSTResult<T, TWeight>(mstEdges, "Prim", component.Count, iterations);
    }

    /// <summary>
    /// Priority queue implementation for Prim's algorithm.
    /// </summary>
    private class PriorityQueue
    {
        private readonly List<WeightedEdge<T, TWeight>> _elements = new();

        public int Count => _elements.Count;

        public void Enqueue(WeightedEdge<T, TWeight> item)
        {
            _elements.Add(item);
            var childIndex = _elements.Count - 1;
            while (childIndex > 0)
            {
                var parentIndex = (childIndex - 1) / 2;
                if (Compare(_elements[childIndex].Weight, _elements[parentIndex].Weight) >= 0)
                    break;

                var temp = _elements[childIndex];
                _elements[childIndex] = _elements[parentIndex];
                _elements[parentIndex] = temp;
                childIndex = parentIndex;
            }
        }

        public WeightedEdge<T, TWeight> Dequeue()
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
                if (rightChildIndex <= lastIndex && Compare(_elements[rightChildIndex].Weight, _elements[leftChildIndex].Weight) < 0)
                    leftChildIndex = rightChildIndex;

                if (Compare(_elements[parentIndex].Weight, _elements[leftChildIndex].Weight) <= 0)
                    break;

                var temp = _elements[parentIndex];
                _elements[parentIndex] = _elements[leftChildIndex];
                _elements[leftChildIndex] = temp;
                parentIndex = leftChildIndex;
            }

            return result;
        }
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
