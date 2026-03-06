using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Coloring;

/// <summary>
/// Implementation of the Welsh-Powell graph coloring algorithm.
/// Uses degree ordering and assigns colors to maximize color reuse.
/// Particularly effective for graphs with high-degree nodes.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public static class WelshPowell<T>
    where T : notnull
{
    /// <summary>
    /// Computes a graph coloring using the Welsh-Powell algorithm.
    /// </summary>
    /// <param name="graph">The graph to color</param>
    /// <returns>The coloring result</returns>
    public static ColoringResult<T> ColorGraph(IGraph<T> graph)
    {
        if (graph.NodeCount == 0)
            throw new ArgumentException("Graph cannot be empty.");

        var colorAssignments = new Dictionary<T, int>();
        var iterations = 0;

        // Step 1: Order vertices by decreasing degree
        var orderedNodes = graph.GetNodes()
            .OrderByDescending(n => graph.GetDegree(n))
            .ToList();

        var currentColor = 1;

        while (colorAssignments.Count < graph.NodeCount)
        {
            iterations++;

            // Step 2: Find uncolored vertices that can be assigned current color
            var colorableNodes = new List<T>();

            foreach (var node in orderedNodes)
            {
                if (!colorAssignments.ContainsKey(node) && CanColorWith(graph, node, currentColor, colorAssignments))
                {
                    colorableNodes.Add(node);
                }
            }

            // Step 3: Assign current color to all colorable nodes
            foreach (var node in colorableNodes)
            {
                colorAssignments[node] = currentColor;
            }

            currentColor++;
        }

        var isValid = ValidateColoring(graph, colorAssignments);
        return new ColoringResult<T>(colorAssignments, "Welsh-Powell", iterations, isValid);
    }

    /// <summary>
    /// Computes a graph coloring using the Welsh-Powell algorithm with optimization.
    /// Includes additional heuristics to improve color assignment.
    /// </summary>
    /// <param name="graph">The graph to color</param>
    /// <param name="useOptimization">Whether to use optimization heuristics</param>
    /// <returns>The coloring result</returns>
    public static ColoringResult<T> ColorGraphOptimized(IGraph<T> graph, bool useOptimization = true)
    {
        if (graph.NodeCount == 0)
            throw new ArgumentException("Graph cannot be empty.");

        var colorAssignments = new Dictionary<T, int>();
        var iterations = 0;

        // Step 1: Order vertices by decreasing degree
        var orderedNodes = graph.GetNodes()
            .OrderByDescending(n => graph.GetDegree(n))
            .ToList();

        var currentColor = 1;

        while (colorAssignments.Count < graph.NodeCount)
        {
            iterations++;

            // Step 2: Find uncolored vertices that can be assigned current color
            var colorableNodes = new List<T>();

            foreach (var node in orderedNodes)
            {
                if (!colorAssignments.ContainsKey(node) && CanColorWith(graph, node, currentColor, colorAssignments))
                {
                    colorableNodes.Add(node);
                }
            }

            // Optimization: Sort colorable nodes by their degree to maximize independence
            if (useOptimization)
            {
                colorableNodes = colorableNodes
                    .OrderByDescending(n => graph.GetDegree(n))
                    .ToList();
            }

            // Step 3: Assign current color to all colorable nodes
            foreach (var node in colorableNodes)
            {
                colorAssignments[node] = currentColor;
            }

            currentColor++;
        }

        // Additional optimization: Try to reduce the number of colors
        if (useOptimization)
        {
            colorAssignments = OptimizeColoring(graph, colorAssignments, ref iterations);
        }

        var isValid = ValidateColoring(graph, colorAssignments);
        return new ColoringResult<T>(colorAssignments, "Welsh-Powell Optimized", iterations, isValid);
    }

    /// <summary>
    /// Estimates the chromatic number using Welsh-Powell bounds.
    /// </summary>
    /// <param name="graph">The graph to analyze</param>
    /// <returns>A tuple with (lower bound, upper bound) for chromatic number</returns>
    public static (int LowerBound, int UpperBound) EstimateChromaticNumber(IGraph<T> graph)
    {
        if (graph.NodeCount == 0)
            return (0, 0);

        // Lower bound: clique number (maximum degree + 1 for complete graphs)
        var maxDegree = graph.GetNodes().Max(n => graph.GetDegree(n));
        var lowerBound = maxDegree + 1;

        // Upper bound: Welsh-Powell bound
        var orderedNodes = graph.GetNodes().OrderByDescending(n => graph.GetDegree(n)).ToList();
        var upperBound = 0;
        var colored = new HashSet<T>();

        foreach (var node in orderedNodes)
        {
            if (!colored.Contains(node))
            {
                upperBound++;
                colored.Add(node);
                foreach (var neighbor in graph.GetNeighbors(node))
                {
                    if (!colored.Contains(neighbor))
                    {
                        colored.Add(neighbor);
                    }
                }
            }
        }

        return (lowerBound, upperBound);
    }

    private static bool CanColorWith(IGraph<T> graph, T node, int color, Dictionary<T, int> colorAssignments)
    {
        foreach (var neighbor in graph.GetNeighbors(node))
        {
            if (colorAssignments.ContainsKey(neighbor) && colorAssignments[neighbor] == color)
            {
                return false;
            }
        }
        return true;
    }

    private static Dictionary<T, int> OptimizeColoring(IGraph<T> graph, Dictionary<T, int> colorAssignments, ref int iterations)
    {
        var optimized = new Dictionary<T, int>(colorAssignments);
        var maxColor = optimized.Values.Distinct().Count();
        var improved = true;

        while (improved)
        {
            improved = false;

            // Try to eliminate the highest color
            for (int targetColor = maxColor; targetColor > 1; targetColor--)
            {
                var nodesWithTargetColor = optimized
                    .Where(kvp => kvp.Value == targetColor)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var node in nodesWithTargetColor)
                {
                    iterations++;

                    // Try to recolor this node with a smaller color
                    for (int newColor = 1; newColor < targetColor; newColor++)
                    {
                        if (CanColorWith(graph, node, newColor, optimized))
                        {
                            optimized[node] = newColor;
                            improved = true;
                            break;
                        }
                    }

                    if (improved) break;
                }

                if (improved) break;
            }

            // Recalculate max color
            maxColor = optimized.Values.Distinct().Count();
        }

        return optimized;
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
