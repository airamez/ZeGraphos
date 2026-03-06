using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.SpanningTree;

/// <summary>
/// Represents the result of a minimum spanning tree algorithm execution.
/// Contains the MST edges, total weight, and algorithm metadata.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class MSTResult<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Gets the edges in the minimum spanning tree.
    /// </summary>
    public IReadOnlyList<WeightedEdge<T, TWeight>> TreeEdges { get; }

    /// <summary>
    /// Gets the total weight of the minimum spanning tree.
    /// </summary>
    public TWeight TotalWeight { get; }

    /// <summary>
    /// Gets the algorithm used to compute the MST.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of nodes in the original graph.
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    /// Gets the number of edges in the MST.
    /// </summary>
    public int EdgeCount => TreeEdges.Count;

    /// <summary>
    /// Gets whether the MST spans all nodes in the original graph.
    /// </summary>
    public bool SpansAllNodes => EdgeCount == NodeCount - 1 || (NodeCount == 1 && EdgeCount == 0);

    /// <summary>
    /// Gets the number of iterations performed during the algorithm execution.
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// Initializes a new instance of the MSTResult class.
    /// </summary>
    /// <param name="treeEdges">The edges in the minimum spanning tree</param>
    /// <param name="algorithm">The algorithm name</param>
    /// <param name="nodeCount">Number of nodes in the original graph</param>
    /// <param name="iterations">Number of iterations performed</param>
    public MSTResult(IEnumerable<WeightedEdge<T, TWeight>> treeEdges, string algorithm, int nodeCount, int iterations)
    {
        TreeEdges = new ReadOnlyCollection<WeightedEdge<T, TWeight>>(treeEdges.ToList());
        Algorithm = algorithm;
        NodeCount = nodeCount;
        Iterations = iterations;
        TotalWeight = CalculateTotalWeight();
    }

    /// <summary>
    /// Gets whether a specific edge is included in the MST.
    /// </summary>
    /// <param name="source">The source node of the edge</param>
    /// <param name="target">The target node of the edge</param>
    /// <returns>True if the edge is in the MST, false otherwise</returns>
    public bool ContainsEdge(T source, T target)
    {
        return TreeEdges.Any(e => 
            (e.Source.Equals(source) && e.Target.Equals(target)) ||
            (e.Source.Equals(target) && e.Target.Equals(source)));
    }

    /// <summary>
    /// Gets the degree of a node in the MST.
    /// </summary>
    /// <param name="node">The node to get degree for</param>
    /// <returns>The degree of the node in the MST</returns>
    public int GetDegree(T node)
    {
        return TreeEdges.Count(e => e.Source.Equals(node) || e.Target.Equals(node));
    }

    /// <summary>
    /// Gets the neighbors of a node in the MST.
    /// </summary>
    /// <param name="node">The node to get neighbors for</param>
    /// <returns>A collection of neighboring nodes in the MST</returns>
    public IReadOnlyCollection<T> GetNeighbors(T node)
    {
        var neighbors = TreeEdges
            .Where(e => e.Source.Equals(node) || e.Target.Equals(node))
            .Select(e => e.Source.Equals(node) ? e.Target : e.Source)
            .ToList();

        return new ReadOnlyCollection<T>(neighbors);
    }

    /// <summary>
    /// Returns a string representation of the MST result.
    /// </summary>
    /// <returns>A string showing the total weight and algorithm used</returns>
    public override string ToString()
    {
        return $"Minimum Spanning Tree using {Algorithm}: {EdgeCount} edges, " +
               $"total weight {TotalWeight} ({Iterations} iterations)";
    }

    private TWeight CalculateTotalWeight()
    {
        if (typeof(TWeight) == typeof(double))
        {
            double total = 0;
            foreach (var edge in TreeEdges)
            {
                total += Convert.ToDouble(edge.Weight);
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(int))
        {
            int total = 0;
            foreach (var edge in TreeEdges)
            {
                total += Convert.ToInt32(edge.Weight);
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(float))
        {
            float total = 0;
            foreach (var edge in TreeEdges)
            {
                total += Convert.ToSingle(edge.Weight);
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else
        {
            // For other numeric types, use dynamic
            dynamic total = default(TWeight);
            foreach (var edge in TreeEdges)
            {
                total += (dynamic)edge.Weight;
            }
            return total;
        }
    }
}

/// <summary>
/// Represents a forest (collection of trees) result from spanning tree algorithms.
/// Used when the input graph is disconnected.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public sealed class ForestResult<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IEquatable<TWeight>, IConvertible
{
    /// <summary>
    /// Gets the collection of trees in the forest.
    /// </summary>
    public IReadOnlyList<MSTResult<T, TWeight>> Trees { get; }

    /// <summary>
    /// Gets the total weight of all trees in the forest.
    /// </summary>
    public TWeight TotalWeight { get; }

    /// <summary>
    /// Gets the algorithm used to compute the forest.
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Gets the number of trees in the forest.
    /// </summary>
    public int TreeCount => Trees.Count;

    /// <summary>
    /// Gets the total number of nodes across all trees.
    /// </summary>
    public int NodeCount => Trees.Sum(t => t.NodeCount);

    /// <summary>
    /// Gets the total number of edges across all trees.
    /// </summary>
    public int EdgeCount => Trees.Sum(t => t.EdgeCount);

    /// <summary>
    /// Initializes a new instance of the ForestResult class.
    /// </summary>
    /// <param name="trees">The collection of trees in the forest</param>
    /// <param name="algorithm">The algorithm name</param>
    public ForestResult(IEnumerable<MSTResult<T, TWeight>> trees, string algorithm)
    {
        Trees = new ReadOnlyCollection<MSTResult<T, TWeight>>(trees.ToList());
        Algorithm = algorithm;
        TotalWeight = CalculateTotalWeight();
    }

    /// <summary>
    /// Returns a string representation of the forest result.
    /// </summary>
    /// <returns>A string showing the number of trees and total weight</returns>
    public override string ToString()
    {
        return $"Minimum Forest using {Algorithm}: {TreeCount} trees, " +
               $"{EdgeCount} edges, total weight {TotalWeight}";
    }

    private TWeight CalculateTotalWeight()
    {
        if (typeof(TWeight) == typeof(double))
        {
            double total = 0;
            foreach (var tree in Trees)
            {
                total += Convert.ToDouble(tree.TotalWeight);
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(int))
        {
            int total = 0;
            foreach (var tree in Trees)
            {
                total += Convert.ToInt32(tree.TotalWeight);
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else if (typeof(TWeight) == typeof(float))
        {
            float total = 0;
            foreach (var tree in Trees)
            {
                total += Convert.ToSingle(tree.TotalWeight);
            }
            return (TWeight)Convert.ChangeType(total, typeof(TWeight));
        }
        else
        {
            // For other numeric types, use dynamic
            dynamic total = default(TWeight);
            foreach (var tree in Trees)
            {
                total += (dynamic)tree.TotalWeight;
            }
            return total;
        }
    }
}
