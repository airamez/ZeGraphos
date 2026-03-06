using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Implementations;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Tests.Core;

/// <summary>
/// Unit tests for core graph functionality.
/// Tests all graph types and basic operations.
/// </summary>
public class GraphTests
{
    #region Directed Graph Tests

    [Fact]
    public void DirectedGraph_ShouldCreateEmptyGraph()
    {
        // Arrange & Act
        var graph = new DirectedGraph<string>();

        // Assert
        graph.NodeCount.Should().Be(0);
        graph.EdgeCount.Should().Be(0);
        graph.IsDirected.Should().BeTrue();
        graph.IsWeighted.Should().BeFalse();
    }

    [Fact]
    public void DirectedGraph_ShouldAddNodes()
    {
        // Arrange
        var graph = new DirectedGraph<string>();

        // Act
        var result1 = graph.AddNode("A");
        var result2 = graph.AddNode("B");
        var result3 = graph.AddNode("A"); // Duplicate

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();
        graph.NodeCount.Should().Be(2);
        graph.ContainsNode("A").Should().BeTrue();
        graph.ContainsNode("B").Should().BeTrue();
    }

    [Fact]
    public void DirectedGraph_ShouldAddEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddNode("A");
        graph.AddNode("B");

        // Act
        var result1 = graph.AddEdge("A", "B");
        var result2 = graph.AddEdge("A", "B"); // Duplicate

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        graph.EdgeCount.Should().Be(1);
        graph.ContainsEdge("A", "B").Should().BeTrue();
        graph.ContainsEdge("B", "A").Should().BeFalse();
    }

    [Fact]
    public void DirectedGraph_ShouldAutoAddNodesWhenAddingEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string>();

        // Act
        var result = graph.AddEdge("A", "B");

        // Assert
        result.Should().BeTrue();
        graph.NodeCount.Should().Be(2);
        graph.EdgeCount.Should().Be(1);
        graph.ContainsNode("A").Should().BeTrue();
        graph.ContainsNode("B").Should().BeTrue();
    }

    [Fact]
    public void DirectedGraph_ShouldRemoveNodes()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        // Act
        var result = graph.RemoveNode("B");

        // Assert
        result.Should().BeTrue();
        graph.NodeCount.Should().Be(2); // A and C remain
        graph.EdgeCount.Should().Be(0); // All edges removed
        graph.ContainsNode("B").Should().BeFalse();
    }

    [Fact]
    public void DirectedGraph_ShouldRemoveEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        // Act
        var result = graph.RemoveEdge("A", "B");

        // Assert
        result.Should().BeTrue();
        graph.EdgeCount.Should().Be(1);
        graph.ContainsEdge("A", "B").Should().BeFalse();
        graph.ContainsEdge("B", "C").Should().BeTrue();
    }

    [Fact]
    public void DirectedGraph_ShouldGetNeighbors()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddEdges(("A", "B"), ("A", "C"), ("B", "D"));

        // Act
        var neighborsA = graph.GetNeighbors("A");
        var neighborsB = graph.GetNeighbors("B");
        var neighborsC = graph.GetNeighbors("C");

        // Assert
        neighborsA.Should().Contain("B");
        neighborsA.Should().Contain("C");
        neighborsA.Should().HaveCount(2);
        
        neighborsB.Should().Contain("D");
        neighborsB.Should().HaveCount(1);
        
        neighborsC.Should().BeEmpty();
    }

    [Fact]
    public void DirectedGraph_ShouldGetDegree()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddEdges(("A", "B"), ("A", "C"), ("B", "C"));

        // Act & Assert
        graph.GetDegree("A").Should().Be(2); // Out-degree
        graph.GetDegree("B").Should().Be(1);
        graph.GetDegree("C").Should().Be(0);
    }

    #endregion

    #region Undirected Graph Tests

    [Fact]
    public void UndirectedGraph_ShouldCreateEmptyGraph()
    {
        // Arrange & Act
        var graph = new UndirectedGraph<string>();

        // Assert
        graph.NodeCount.Should().Be(0);
        graph.EdgeCount.Should().Be(0);
        graph.IsDirected.Should().BeFalse();
        graph.IsWeighted.Should().BeFalse();
    }

    [Fact]
    public void UndirectedGraph_ShouldAddBidirectionalEdges()
    {
        // Arrange
        var graph = new UndirectedGraph<string>();
        graph.AddNode("A");
        graph.AddNode("B");

        // Act
        var result = graph.AddEdge("A", "B");

        // Assert
        result.Should().BeTrue();
        graph.EdgeCount.Should().Be(1);
        graph.ContainsEdge("A", "B").Should().BeTrue();
        graph.ContainsEdge("B", "A").Should().BeTrue();
    }

    [Fact]
    public void UndirectedGraph_ShouldPreventSelfLoops()
    {
        // Arrange
        var graph = new UndirectedGraph<string>();
        graph.AddNode("A");

        // Act
        var result = graph.AddEdge("A", "A");

        // Assert
        result.Should().BeFalse();
        graph.EdgeCount.Should().Be(0);
    }

    [Fact]
    public void UndirectedGraph_ShouldGetNeighbors()
    {
        // Arrange
        var graph = new UndirectedGraph<string>();
        graph.AddEdges(("A", "B"), ("A", "C"), ("B", "D"));

        // Act
        var neighborsA = graph.GetNeighbors("A");
        var neighborsB = graph.GetNeighbors("B");

        // Assert
        neighborsA.Should().Contain("B");
        neighborsA.Should().Contain("C");
        neighborsA.Should().HaveCount(2);
        
        neighborsB.Should().Contain("A");
        neighborsB.Should().Contain("D");
        neighborsB.Should().HaveCount(2);
    }

    #endregion

    #region Weighted Directed Graph Tests

    [Fact]
    public void WeightedDirectedGraph_ShouldCreateEmptyGraph()
    {
        // Arrange & Act
        var graph = new WeightedDirectedGraph<string, double>();

        // Assert
        graph.NodeCount.Should().Be(0);
        graph.EdgeCount.Should().Be(0);
        graph.IsDirected.Should().BeTrue();
        graph.IsWeighted.Should().BeTrue();
    }

    [Fact]
    public void WeightedDirectedGraph_ShouldAddWeightedEdges()
    {
        // Arrange
        var graph = new WeightedDirectedGraph<string, double>();

        // Act
        var result = graph.AddEdge("A", "B", 2.5);

        // Assert
        result.Should().BeTrue();
        graph.EdgeCount.Should().Be(1);
        graph.GetEdgeWeight("A", "B").Should().Be(2.5);
    }

    [Fact]
    public void WeightedDirectedGraph_ShouldThrowOnUnweightedAddEdge()
    {
        // Arrange
        var graph = new WeightedDirectedGraph<string, double>();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => graph.AddEdge("A", "B"));
    }

    [Fact]
    public void WeightedDirectedGraph_ShouldSetAndGetEdgeWeights()
    {
        // Arrange
        var graph = new WeightedDirectedGraph<string, double>();
        graph.AddEdge("A", "B", 1.0);

        // Act
        graph.SetEdgeWeight("A", "B", 3.14);
        var weight = graph.GetEdgeWeight("A", "B");

        // Assert
        weight.Should().Be(3.14);
    }

    [Fact]
    public void WeightedDirectedGraph_ShouldTryGetEdgeWeight()
    {
        // Arrange
        var graph = new WeightedDirectedGraph<string, double>();
        graph.AddEdge("A", "B", 2.5);

        // Act
        var result1 = graph.TryGetEdgeWeight("A", "B", out var weight1);
        var result2 = graph.TryGetEdgeWeight("A", "C", out var weight2);

        // Assert
        result1.Should().BeTrue();
        weight1.Should().Be(2.5);
        
        result2.Should().BeFalse();
        weight2.Should().Be(0);
    }

    #endregion

    #region Weighted Undirected Graph Tests

    [Fact]
    public void WeightedUndirectedGraph_ShouldAddBidirectionalWeightedEdges()
    {
        // Arrange
        var graph = new WeightedUndirectedGraph<string, double>();

        // Act
        var result = graph.AddEdge("A", "B", 2.5);

        // Assert
        result.Should().BeTrue();
        graph.EdgeCount.Should().Be(1);
        graph.GetEdgeWeight("A", "B").Should().Be(2.5);
        graph.GetEdgeWeight("B", "A").Should().Be(2.5);
    }

    [Fact]
    public void WeightedUndirectedGraph_ShouldSynchronizeWeights()
    {
        // Arrange
        var graph = new WeightedUndirectedGraph<string, double>();
        graph.AddEdge("A", "B", 1.0);

        // Act
        graph.SetEdgeWeight("A", "B", 5.0);

        // Assert
        graph.GetEdgeWeight("A", "B").Should().Be(5.0);
        graph.GetEdgeWeight("B", "A").Should().Be(5.0);
    }

    #endregion

    #region Graph Builder Tests

    [Fact]
    public void GraphBuilder_ShouldCreateDifferentGraphTypes()
    {
        // Act
        var directed = GraphBuilder.CreateDirected<string>();
        var undirected = GraphBuilder.CreateUndirected<string>();
        var weightedDirected = GraphBuilder.CreateWeightedDirected<string, double>();
        var weightedUndirected = GraphBuilder.CreateWeightedUndirected<string, double>();

        // Assert
        directed.Should().BeOfType<DirectedGraph<string>>();
        undirected.Should().BeOfType<UndirectedGraph<string>>();
        weightedDirected.Should().BeOfType<WeightedDirectedGraph<string, double>>();
        weightedUndirected.Should().BeOfType<WeightedUndirectedGraph<string, double>>();
    }

    [Fact]
    public void GraphBuilderExtensions_ShouldSupportFluentChaining()
    {
        // Arrange
        var graph = GraphBuilder.CreateUndirected<string>();

        // Act
        var result = graph
            .AddNodes("A", "B", "C", "D")
            .AddEdges(("A", "B"), ("B", "C"), ("C", "D"))
            .MakeStar("B", "A", "C", "D");

        // Assert
        result.Should().BeSameAs(graph);
        graph.NodeCount.Should().Be(4);
        graph.EdgeCount.Should().Be(4); // 3 from initial edges + 1 from star (B is center)
    }

    [Fact]
    public void GraphBuilderExtensions_ShouldCreateCompleteGraph()
    {
        // Arrange
        var graph = GraphBuilder.CreateDirected<string>();

        // Act
        graph.MakeComplete("A", "B", "C");

        // Assert
        graph.NodeCount.Should().Be(3);
        graph.EdgeCount.Should().Be(6); // 3 * 2 = 6 for directed complete graph
    }

    [Fact]
    public void GraphBuilderExtensions_ShouldCreateCycle()
    {
        // Arrange
        var graph = GraphBuilder.CreateUndirected<string>();

        // Act
        graph.MakeCycle("A", "B", "C", "D");

        // Assert
        graph.NodeCount.Should().Be(4);
        graph.EdgeCount.Should().Be(4);
        graph.ContainsEdge("A", "B").Should().BeTrue();
        graph.ContainsEdge("B", "C").Should().BeTrue();
        graph.ContainsEdge("C", "D").Should().BeTrue();
        graph.ContainsEdge("D", "A").Should().BeTrue();
    }

    [Fact]
    public void GraphBuilderExtensions_ShouldCreatePath()
    {
        // Arrange
        var graph = GraphBuilder.CreateUndirected<string>();

        // Act
        graph.MakePath("A", "B", "C", "D");

        // Assert
        graph.NodeCount.Should().Be(4);
        graph.EdgeCount.Should().Be(3);
        graph.ContainsEdge("A", "B").Should().BeTrue();
        graph.ContainsEdge("B", "C").Should().BeTrue();
        graph.ContainsEdge("C", "D").Should().BeTrue();
    }

    [Fact]
    public void GraphBuilderExtensions_ShouldCreateStar()
    {
        // Arrange
        var graph = GraphBuilder.CreateUndirected<string>();

        // Act
        graph.MakeStar("Center", "Leaf1", "Leaf2", "Leaf3");

        // Assert
        graph.NodeCount.Should().Be(4);
        graph.EdgeCount.Should().Be(3);
        graph.ContainsEdge("Center", "Leaf1").Should().BeTrue();
        graph.ContainsEdge("Center", "Leaf2").Should().BeTrue();
        graph.ContainsEdge("Center", "Leaf3").Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Graph_ShouldHandleEmptyGraph()
    {
        // Arrange
        var graph = new DirectedGraph<string>();

        // Act & Assert
        graph.NodeCount.Should().Be(0);
        graph.EdgeCount.Should().Be(0);
        graph.GetNodes().Should().BeEmpty();
        graph.GetEdges().Should().BeEmpty();
    }

    [Fact]
    public void Graph_ShouldHandleSingleNode()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddNode("A");

        // Act & Assert
        graph.NodeCount.Should().Be(1);
        graph.EdgeCount.Should().Be(0);
        graph.GetNeighbors("A").Should().BeEmpty();
        graph.GetDegree("A").Should().Be(0);
    }

    [Fact]
    public void Graph_ShouldThrowOnNonExistentNode()
    {
        // Arrange
        var graph = new DirectedGraph<string>();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => graph.GetNeighbors("NonExistent"));
        Assert.Throws<KeyNotFoundException>(() => graph.GetDegree("NonExistent"));
    }

    [Fact]
    public void Graph_ShouldClearAllData()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddEdges(("A", "B"), ("B", "C"));

        // Act
        graph.Clear();

        // Assert
        graph.NodeCount.Should().Be(0);
        graph.EdgeCount.Should().Be(0);
        graph.GetNodes().Should().BeEmpty();
    }

    #endregion

    #region Edge and Node Tests

    [Fact]
    public void Edge_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var edge = new Edge<string>("A", "B");

        // Assert
        edge.Source.Should().Be("A");
        edge.Target.Should().Be("B");
        edge.IsSelfLoop.Should().BeFalse();
        edge.ToString().Should().Be("A -> B");
    }

    [Fact]
    public void Edge_ShouldHandleSelfLoop()
    {
        // Arrange & Act
        var edge = new Edge<string>("A", "A");

        // Assert
        edge.Source.Should().Be("A");
        edge.Target.Should().Be("A");
        edge.IsSelfLoop.Should().BeTrue();
    }

    [Fact]
    public void WeightedEdge_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var edge = new WeightedEdge<string, double>("A", "B", 2.5);

        // Assert
        edge.Source.Should().Be("A");
        edge.Target.Should().Be("B");
        edge.Weight.Should().Be(2.5);
        edge.ToString().Should().Be("A -> B (2.5)");
    }

    [Fact]
    public void WeightedEdge_ShouldReverseCorrectly()
    {
        // Arrange & Act
        var edge = new WeightedEdge<string, double>("A", "B", 2.5);
        var reversed = edge.Reverse();

        // Assert
        reversed.Source.Should().Be("B");
        reversed.Target.Should().Be("A");
        reversed.Weight.Should().Be(2.5);
    }

    #endregion
}
