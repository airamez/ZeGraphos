using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.Coloring;

/// <summary>
/// Implementation of greedy graph coloring algorithms with various ordering strategies.
/// Assigns colors to nodes sequentially, always using the smallest available color.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
public static class GreedyColoring<T>
    where T : notnull
{
    /// <summary>
    /// Computes a graph coloring using the greedy algorithm with specified ordering.
    /// </summary>
    /// <param name="graph">The graph to color</param>
    /// <param name="ordering">The ordering strategy for nodes</param>
    /// <param name="strategy">The coloring strategy</param>
    /// <returns>The coloring result</returns>
    public static ColoringResult<T> ColorGraph(IGraph<T> graph, ColoringOrdering ordering = ColoringOrdering.DegreeDescending, ColoringStrategy strategy = ColoringStrategy.FirstAvailable)
    {
        if (graph.NodeCount == 0)
            throw new ArgumentException("Graph cannot be empty.");

        var colorAssignments = new Dictionary<T, int>();
        var iterations = 0;
        var orderingFunction = GetOrderingFunction(ordering);
        var coloringFunction = GetColoringFunction(strategy);

        // Get nodes in the specified order
        var orderedNodes = orderingFunction(graph).ToList();

        foreach (var node in orderedNodes)
        {
            iterations++;
            var color = coloringFunction(graph, node, colorAssignments);
            colorAssignments[node] = color;
        }

        // Validate the coloring
        var isValid = ValidateColoring(graph, colorAssignments);

        return new ColoringResult<T>(colorAssignments, $"Greedy ({ordering}, {strategy})", iterations, isValid);
    }

    /// <summary>
    /// Computes a coloring with the optimal ordering for the given graph type.
    /// </summary>
    /// <param name="graph">The graph to color</param>
    /// <returns>The coloring result</returns>
    public static ColoringResult<T> ColorGraphOptimal(IGraph<T> graph)
    {
        // Choose ordering based on graph properties
        var ordering = graph.IsDirected ? ColoringOrdering.DegreeDescending : ColoringOrdering.LargestDegreeFirst;
        var strategy = graph.NodeCount > 100 ? ColoringStrategy.FirstAvailable : ColoringStrategy.LeastConflict;

        return ColorGraph(graph, ordering, strategy);
    }

    /// <summary>
    /// Attempts to improve an existing coloring by recoloring nodes.
    /// </summary>
    /// <param name="graph">The graph</param>
    /// <param name="initialColoring">The initial coloring to improve</param>
    /// <param name="maxIterations">Maximum number of improvement iterations</param>
    /// <returns>The improved coloring result</returns>
    public static ColoringResult<T> ImproveColoring(IGraph<T> graph, ColoringResult<T> initialColoring, int maxIterations = 100)
    {
        var colorAssignments = new Dictionary<T, int>(initialColoring.ColorAssignments);
        var iterations = 0;
        var improved = true;

        while (improved && iterations < maxIterations)
        {
            improved = false;
            iterations++;

            // Try to recolor each node
            foreach (var node in graph.GetNodes())
            {
                var currentColor = colorAssignments[node];
                var availableColors = GetAvailableColors(graph, node, colorAssignments);
                
                // Try to use a smaller color
                var smallerColor = availableColors.FirstOrDefault(c => c < currentColor);
                if (smallerColor > 0)
                {
                    colorAssignments[node] = smallerColor;
                    improved = true;
                }
            }
        }

        var isValid = ValidateColoring(graph, colorAssignments);
        return new ColoringResult<T>(colorAssignments, $"Greedy Improved", iterations, isValid);
    }

    private static Func<IGraph<T>, IEnumerable<T>> GetOrderingFunction(ColoringOrdering ordering)
    {
        return ordering switch
        {
            ColoringOrdering.Natural => graph => graph.GetNodes(),
            ColoringOrdering.DegreeDescending => graph => graph.GetNodes().OrderByDescending(n => graph.GetDegree(n)),
            ColoringOrdering.DegreeAscending => graph => graph.GetNodes().OrderBy(n => graph.GetDegree(n)),
            ColoringOrdering.LargestDegreeFirst => graph => graph.GetNodes().OrderByDescending(n => graph.GetDegree(n)),
            ColoringOrdering.SmallestDegreeLast => graph => graph.GetNodes().OrderBy(n => graph.GetDegree(n)),
            ColoringOrdering.Random => graph => graph.GetNodes().OrderBy(_ => Guid.NewGuid()),
            _ => graph => graph.GetNodes()
        };
    }

    private static Func<IGraph<T>, T, Dictionary<T, int>, int> GetColoringFunction(ColoringStrategy strategy)
    {
        return strategy switch
        {
            ColoringStrategy.FirstAvailable => GetFirstAvailableColor,
            ColoringStrategy.LeastConflict => GetLeastConflictColor,
            ColoringStrategy.MaximumDistance => GetMaximumDistanceColor,
            _ => GetFirstAvailableColor
        };
    }

    private static int GetFirstAvailableColor(IGraph<T> graph, T node, Dictionary<T, int> colorAssignments)
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

    private static int GetLeastConflictColor(IGraph<T> graph, T node, Dictionary<T, int> colorAssignments)
    {
        var availableColors = GetAvailableColors(graph, node, colorAssignments);
        
        if (availableColors.Count == 0)
            return GetFirstAvailableColor(graph, node, colorAssignments);

        // Count conflicts for each available color
        var conflicts = new Dictionary<int, int>();
        foreach (var color in availableColors)
        {
            conflicts[color] = 0;
            foreach (var neighbor in graph.GetNeighbors(node))
            {
                if (!colorAssignments.ContainsKey(neighbor))
                {
                    // Uncolored neighbor might conflict with this color
                    var neighborColors = GetAvailableColors(graph, neighbor, colorAssignments);
                    if (!neighborColors.Contains(color))
                        conflicts[color]++;
                }
            }
        }

        // Choose color with minimum conflicts
        return conflicts.OrderBy(kvp => kvp.Value).First().Key;
    }

    private static int GetMaximumDistanceColor(IGraph<T> graph, T node, Dictionary<T, int> colorAssignments)
    {
        var availableColors = GetAvailableColors(graph, node, colorAssignments);
        
        if (availableColors.Count == 0)
            return GetFirstAvailableColor(graph, node, colorAssignments);

        var usedColors = colorAssignments.Values.Distinct().ToList();
        if (usedColors.Count == 0)
            return 1;

        // Choose color that maximizes distance from used colors
        var maxDistance = -1;
        var bestColor = availableColors.First();

        foreach (var color in availableColors)
        {
            var minDistance = usedColors.Min(c => Math.Abs(c - color));
            if (minDistance > maxDistance)
            {
                maxDistance = minDistance;
                bestColor = color;
            }
        }

        return bestColor;
    }

    private static HashSet<int> GetAvailableColors(IGraph<T> graph, T node, Dictionary<T, int> colorAssignments)
    {
        var usedColors = new HashSet<int>();

        foreach (var neighbor in graph.GetNeighbors(node))
        {
            if (colorAssignments.ContainsKey(neighbor))
            {
                usedColors.Add(colorAssignments[neighbor]);
            }
        }

        var availableColors = new HashSet<int>();
        var maxUsedColor = usedColors.Count > 0 ? usedColors.Max() : 0;
        var maxAssignedColor = colorAssignments.Count > 0 ? colorAssignments.Values.Max() : 0;
        var maxColor = Math.Max(maxUsedColor, maxAssignedColor);

        for (int color = 1; color <= maxColor + 1; color++)
        {
            if (!usedColors.Contains(color))
            {
                availableColors.Add(color);
            }
        }

        return availableColors;
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
