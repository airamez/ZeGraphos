using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using ZeGraphos.Core.Implementations;
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Algorithms.Flow;
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Algorithms.TopologicalSort;
using ZeGraphos.Extensions;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Tests.Algorithms;

/// <summary>
/// Simplified algorithm validation tests with known results.
/// These tests verify that algorithms produce expected results on classic examples.
/// </summary>
public class AlgorithmValidationTests
{
    #region Shortest Path Validation

    [Fact]
    public void Dijkstra_ShouldFindOptimalPath_TriangleGraph()
    {
        // Arrange - Triangle: A-2-B, A-5-C, B-3-D, C-1-D
        var graph = GraphBuilder.CreateWeightedUndirected<string, double>();
        graph.AddEdge("A", "B", 2.0);
        graph.AddEdge("A", "C", 5.0);
        graph.AddEdge("B", "D", 3.0);
        graph.AddEdge("C", "D", 1.0);

        // Act
        var result = Dijkstra<string, double>.FindShortestPath(graph, "A", "D");

        // Assert - Should find A-B-D with total weight 5
        result.Path.Should().BeEquivalentTo(new[] { "A", "B", "D" });
        result.Distance.Should().Be(5.0);
        result.Algorithm.Should().Be("Dijkstra");
    }

    [Fact]
    public void BFS_ShouldFindShortestPath_UnweightedGraph()
    {
        // Arrange - Path: A-B-C-D
        var graph = GraphBuilder.CreateUndirected<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "D");

        // Act
        var result = BFS<string>.FindShortestPath(graph, "A", "D");

        // Assert - Should find A-B-C-D with distance 3
        result.Path.Should().BeEquivalentTo(new[] { "A", "B", "C", "D" });
        result.Distance.Should().Be(3);
        result.Algorithm.Should().Be("BFS");
    }

    #endregion

    #region Maximum Flow Validation

    // TODO: Fix Ford-Fulkerson algorithm implementation
    // [Fact]
    // public void FordFulkerson_ShouldComputeMaxFlow_SimpleNetwork()
    // {
    //     // Arrange - Simple linear network: S-5-A-5-T
    //     var graph = GraphBuilder.CreateWeightedDirected<string, int>();
    //     graph.AddEdge("S", "A", 5);
    //     graph.AddEdge("A", "T", 5);
    //
    //     // Act
    //     var result = FordFulkerson<string, int>.ComputeMaxFlow(graph, "S", "T");
    //
    //     // Assert - Max flow should be 5 (limited by bottleneck)
    //     result.MaxFlow.Should().Be(5);
    //     result.Algorithm.Should().Be("Ford-Fulkerson");
    // }

    #endregion

    #region Minimum Spanning Tree Validation

    [Fact]
    public void Kruskal_ShouldFindMST_SimpleGraph()
    {
        // Arrange - Triangle with weights: A-B(2), A-C(3), B-C(4)
        var graph = GraphBuilder.CreateWeightedUndirected<string, int>();
        graph.AddEdge("A", "B", 2);
        graph.AddEdge("A", "C", 3);
        graph.AddEdge("B", "C", 4);

        // Act
        var result = Kruskal<string, int>.ComputeMST(graph);

        // Assert - Should pick A-B and A-C with total weight 5
        result.TotalWeight.Should().Be(5);
        result.TreeEdges.Should().HaveCount(2);
        result.SpansAllNodes.Should().BeTrue();
        result.Algorithm.Should().Be("Kruskal");
    }

    #endregion

    #region Graph Coloring Validation

    [Fact]
    public void GreedyColoring_ShouldColorCompleteGraph()
    {
        // Arrange - Complete graph K3 needs 3 colors
        var graph = GraphBuilder.CreateUndirected<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "C");

        // Act
        var result = GreedyColoring<string>.ColorGraph(graph);

        // Assert
        result.ChromaticNumber.Should().Be(3);
        result.IsValid.Should().BeTrue();
        result.Algorithm.Should().Contain("Greedy");
    }

    #endregion

    #region Topological Sort Validation

    [Fact]
    public void Kahn_ShouldSortSimpleDAG()
    {
        // Arrange - Simple chain: A -> B -> C
        var graph = GraphBuilder.CreateDirected<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        // Act
        var result = Kahn<string>.Sort(graph);

        // Assert
        result.IsDAG.Should().BeTrue();
        result.IsComplete.Should().BeTrue();
        result.SortedOrder.Should().BeEquivalentTo(new[] { "A", "B", "C" });
        result.Algorithm.Should().Be("Kahn");
    }

    #endregion
}
