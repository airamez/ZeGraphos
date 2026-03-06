using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Coloring;

/// <summary>
/// Implementation of the DSATUR (Degree of Saturation) graph coloring algorithm.
/// Uses saturation degree ordering for optimal color assignment.
/// Often produces better results than greedy algorithms.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public static class DSATUR<T>
    where T : notnull
{
    /// <summary>
    /// Computes a graph coloring using the DSATUR algorithm.
    /// </summary>
    /// <param name="graph">The graph to color</param>
    /// <returns>The coloring result</returns>
    public static ColoringResult<T> ColorGraph(IGraph<T> graph)
    {
        if (graph.NodeCount == 0)
            throw new ArgumentException("Graph cannot be empty.");

        var colorAssignments = new Dictionary<T, int>();
        var saturationDegree = new Dictionary<T, int>();
        var iterations = 0;

        // Initialize saturation degrees
        foreach (var node in graph.GetNodes())
        {
            saturationDegree[node] = 0;
        }

        while (colorAssignments.Count < graph.NodeCount)
        {
            iterations++;

            // Step 1: Select vertex with highest saturation degree
            var selectedNode = SelectNextNode(graph, colorAssignments, saturationDegree);

            // Step 2: Assign the smallest available color
            var color = GetSmallestAvailableColor(graph, selectedNode, colorAssignments);
            colorAssignments[selectedNode] = color;

            // Step 3: Update saturation degrees of uncolored neighbors
            UpdateSaturationDegrees(graph, selectedNode, colorAssignments, saturationDegree);
        }

        var isValid = ValidateColoring(graph, colorAssignments);
        return new ColoringResult<T>(colorAssignments, "DSATUR", iterations, isValid);
    }

    /// <summary>
    /// Computes a graph coloring using the DSATUR algorithm with tie-breaking heuristics.
    /// </summary>
    /// <param name="graph">The graph to color</param>
    /// <param name="useDegreeTieBreaker">Whether to use degree as tie-breaker</param>
    /// <param name="useRandomTieBreaker">Whether to use random tie-breaking for equal saturation</param>
    /// <returns>The coloring result</returns>
    public static ColoringResult<T> ColorGraphAdvanced(IGraph<T> graph, bool useDegreeTieBreaker = true, bool useRandomTieBreaker = false)
    {
        if (graph.NodeCount == 0)
            throw new ArgumentException("Graph cannot be empty.");

        var colorAssignments = new Dictionary<T, int>();
        var saturationDegree = new Dictionary<T, int>();
        var iterations = 0;
        var random = new Random();

        // Initialize saturation degrees
        foreach (var node in graph.GetNodes())
        {
            saturationDegree[node] = 0;
        }

        while (colorAssignments.Count < graph.NodeCount)
        {
            iterations++;

            // Step 1: Select vertex with highest saturation degree (with tie-breaking)
            var selectedNode = SelectNextNodeAdvanced(graph, colorAssignments, saturationDegree, 
                useDegreeTieBreaker, useRandomTieBreaker, random);

            // Step 2: Assign the smallest available color
            var color = GetSmallestAvailableColor(graph, selectedNode, colorAssignments);
            colorAssignments[selectedNode] = color;

            // Step 3: Update saturation degrees of uncolored neighbors
            UpdateSaturationDegrees(graph, selectedNode, colorAssignments, saturationDegree);
        }

        var isValid = ValidateColoring(graph, colorAssignments);
        var algorithmName = "DSATUR";
        if (useDegreeTieBreaker) algorithmName += " (Degree)";
        if (useRandomTieBreaker) algorithmName += " (Random)";

        return new ColoringResult<T>(colorAssignments, algorithmName, iterations, isValid);
    }

    /// <summary>
    /// Estimates the chromatic number using DSATUR-based bounds.
    /// </summary>
    /// <param name="graph">The graph to analyze</param>
    /// <returns>A tuple with (lower bound, upper bound) for chromatic number</returns>
    public static (int LowerBound, int UpperBound) EstimateChromaticNumber(IGraph<T> graph)
    {
        if (graph.NodeCount == 0)
            return (0, 0);

        // Lower bound: maximum degree + 1 for complete graphs, or clique size estimation
        var maxDegree = graph.GetNodes().Max(n => graph.GetDegree(n));
        var lowerBound = maxDegree + 1;

        // Upper bound: Run DSATUR and count colors used
        var coloringResult = ColorGraph(graph);
        var upperBound = coloringResult.ChromaticNumber;

        return (lowerBound, upperBound);
    }

    private static T SelectNextNode(IGraph<T> graph, Dictionary<T, int> colorAssignments, Dictionary<T, int> saturationDegree)
    {
        var uncoloredNodes = graph.GetNodes().Where(n => !colorAssignments.ContainsKey(n)).ToList();

        if (uncoloredNodes.Count == 1)
            return uncoloredNodes[0];

        // Find maximum saturation degree
        var maxSaturation = uncoloredNodes.Max(n => saturationDegree[n]);
        var maxSaturationNodes = uncoloredNodes.Where(n => saturationDegree[n] == maxSaturation).ToList();

        if (maxSaturationNodes.Count == 1)
            return maxSaturationNodes[0];

        // Tie-breaker: select node with highest degree
        return maxSaturationNodes.OrderByDescending(n => graph.GetDegree(n)).First();
    }

    private static T SelectNextNodeAdvanced(IGraph<T> graph, Dictionary<T, int> colorAssignments, 
        Dictionary<T, int> saturationDegree, bool useDegreeTieBreaker, bool useRandomTieBreaker, Random random)
    {
        var uncoloredNodes = graph.GetNodes().Where(n => !colorAssignments.ContainsKey(n)).ToList();

        if (uncoloredNodes.Count == 1)
            return uncoloredNodes[0];

        // Find maximum saturation degree
        var maxSaturation = uncoloredNodes.Max(n => saturationDegree[n]);
        var maxSaturationNodes = uncoloredNodes.Where(n => saturationDegree[n] == maxSaturation).ToList();

        if (maxSaturationNodes.Count == 1)
            return maxSaturationNodes[0];

        // Apply tie-breaking strategy
        if (useRandomTieBreaker)
        {
            return maxSaturationNodes[random.Next(maxSaturationNodes.Count)];
        }
        else if (useDegreeTieBreaker)
        {
            return maxSaturationNodes.OrderByDescending(n => graph.GetDegree(n)).First();
        }
        else
        {
            return maxSaturationNodes.First();
        }
    }

    private static int GetSmallestAvailableColor(IGraph<T> graph, T node, Dictionary<T, int> colorAssignments)
    {
        var usedColors = new HashSet<int>();

        foreach (var neighbor in graph.GetNeighbors(node))
        {
            if (colorAssignments.ContainsKey(neighbor))
            {
                usedColors.Add(colorAssignments[neighbor]);
            }
        }

        var color = 1;
        while (usedColors.Contains(color))
        {
            color++;
        }

        return color;
    }

    private static void UpdateSaturationDegrees(IGraph<T> graph, T coloredNode, 
        Dictionary<T, int> colorAssignments, Dictionary<T, int> saturationDegree)
    {
        var coloredNodeColor = colorAssignments[coloredNode];

        foreach (var neighbor in graph.GetNeighbors(coloredNode))
        {
            if (!colorAssignments.ContainsKey(neighbor))
            {
                // Count distinct colors among colored neighbors of this neighbor
                var neighborColors = new HashSet<int>();
                foreach (var neighborOfNeighbor in graph.GetNeighbors(neighbor))
                {
                    if (colorAssignments.ContainsKey(neighborOfNeighbor))
                    {
                        neighborColors.Add(colorAssignments[neighborOfNeighbor]);
                    }
                }
                saturationDegree[neighbor] = neighborColors.Count;
            }
        }
    }

    private static bool ValidateColoring(IGraph<T> graph, Dictionary<T, int> colorAssignments)
    {
        foreach (var edge in graph.GetEdges())
        {
            if (colorAssignments.ContainsKey(edge.Source) && 
                colorAssignments.ContainsKey(edge.Target) &&
                colorAssignments[edge.Source].Equals(colorAssignments[edge.Target]))
            {
                return false;
            }
        }
        return true;
    }
}
