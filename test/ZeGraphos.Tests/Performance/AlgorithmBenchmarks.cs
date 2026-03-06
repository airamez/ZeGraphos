using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ZeGraphos.Core.Implementations;
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Algorithms.TopologicalSort;
using ZeGraphos.Extensions;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Tests.Performance;

/// <summary>
/// Performance benchmarks for ZeGraphos algorithms.
/// Tests performance on various graph sizes and complexities.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class AlgorithmBenchmarks
{
    private const int SmallGraphSize = 100;
    private const int MediumGraphSize = 1000;
    private const int LargeGraphSize = 5000;

    // Graph instances for benchmarking
    private WeightedUndirectedGraph<string, double> _smallWeightedGraph = null!;
    private WeightedUndirectedGraph<string, double> _mediumWeightedGraph = null!;
    private WeightedUndirectedGraph<string, double> _largeWeightedGraph = null!;
    
    private UndirectedGraph<string> _smallUnweightedGraph = null!;
    private UndirectedGraph<string> _mediumUnweightedGraph = null!;
    private UndirectedGraph<string> _largeUnweightedGraph = null!;
    
    private DirectedGraph<string> _smallDirectedGraph = null!;
    private DirectedGraph<string> _mediumDirectedGraph = null!;
    private DirectedGraph<string> _largeDirectedGraph = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create test graphs of different sizes
        _smallWeightedGraph = CreateWeightedGraph(SmallGraphSize);
        _mediumWeightedGraph = CreateWeightedGraph(MediumGraphSize);
        _largeWeightedGraph = CreateWeightedGraph(LargeGraphSize);
        
        _smallUnweightedGraph = CreateUnweightedGraph(SmallGraphSize);
        _mediumUnweightedGraph = CreateUnweightedGraph(MediumGraphSize);
        _largeUnweightedGraph = CreateUnweightedGraph(LargeGraphSize);
        
        _smallDirectedGraph = CreateDirectedGraph(SmallGraphSize);
        _mediumDirectedGraph = CreateDirectedGraph(MediumGraphSize);
        _largeDirectedGraph = CreateDirectedGraph(LargeGraphSize);
    }

    #region Shortest Path Benchmarks

    [Benchmark]
    public void Dijkstra_SmallGraph()
    {
        var source = "0";
        var target = (SmallGraphSize - 1).ToString();
        Dijkstra<string, double>.FindShortestPath(_smallWeightedGraph, source, target);
    }

    [Benchmark]
    public void Dijkstra_MediumGraph()
    {
        var source = "0";
        var target = (MediumGraphSize - 1).ToString();
        Dijkstra<string, double>.FindShortestPath(_mediumWeightedGraph, source, target);
    }

    [Benchmark]
    public void BFS_SmallGraph()
    {
        var source = "0";
        var target = (SmallGraphSize - 1).ToString();
        BFS<string>.FindShortestPath(_smallUnweightedGraph, source, target);
    }

    [Benchmark]
    public void BFS_MediumGraph()
    {
        var source = "0";
        var target = (MediumGraphSize - 1).ToString();
        BFS<string>.FindShortestPath(_mediumUnweightedGraph, source, target);
    }

    #endregion

    #region Minimum Spanning Tree Benchmarks

    [Benchmark]
    public void Kruskal_SmallGraph()
    {
        Kruskal<string, double>.ComputeMST(_smallWeightedGraph);
    }

    [Benchmark]
    public void Kruskal_MediumGraph()
    {
        Kruskal<string, double>.ComputeMST(_mediumWeightedGraph);
    }

    [Benchmark]
    public void Prim_SmallGraph()
    {
        Prim<string, double>.ComputeMST(_smallWeightedGraph);
    }

    [Benchmark]
    public void Prim_MediumGraph()
    {
        Prim<string, double>.ComputeMST(_mediumWeightedGraph);
    }

    [Benchmark]
    public void Boruvka_SmallGraph()
    {
        Boruvka<string, double>.ComputeMST(_smallWeightedGraph);
    }

    [Benchmark]
    public void Boruvka_MediumGraph()
    {
        Boruvka<string, double>.ComputeMST(_mediumWeightedGraph);
    }

    #endregion

    #region Graph Coloring Benchmarks

    [Benchmark]
    public void GreedyColoring_SmallGraph()
    {
        GreedyColoring<string>.ColorGraph(_smallUnweightedGraph);
    }

    [Benchmark]
    public void GreedyColoring_MediumGraph()
    {
        GreedyColoring<string>.ColorGraph(_mediumUnweightedGraph);
    }

    [Benchmark]
    public void WelshPowell_SmallGraph()
    {
        WelshPowell<string>.ColorGraph(_smallUnweightedGraph);
    }

    [Benchmark]
    public void WelshPowell_MediumGraph()
    {
        WelshPowell<string>.ColorGraph(_mediumUnweightedGraph);
    }

    [Benchmark]
    public void DSATUR_SmallGraph()
    {
        DSATUR<string>.ColorGraph(_smallUnweightedGraph);
    }

    [Benchmark]
    public void DSATUR_MediumGraph()
    {
        DSATUR<string>.ColorGraph(_mediumUnweightedGraph);
    }

    #endregion

    #region Topological Sort Benchmarks

    [Benchmark]
    public void Kahn_SmallDAG()
    {
        Kahn<string>.Sort(_smallDirectedGraph);
    }

    [Benchmark]
    public void Kahn_MediumDAG()
    {
        Kahn<string>.Sort(_mediumDirectedGraph);
    }

    [Benchmark]
    public void DFS_SmallDAG()
    {
        DFSTopologicalSort<string>.Sort(_smallDirectedGraph);
    }

    [Benchmark]
    public void DFS_MediumDAG()
    {
        DFSTopologicalSort<string>.Sort(_mediumDirectedGraph);
    }

    #endregion

    #region Graph Creation Benchmarks

    [Benchmark]
    public void CreateWeightedGraph_Small()
    {
        CreateWeightedGraph(SmallGraphSize);
    }

    [Benchmark]
    public void CreateWeightedGraph_Medium()
    {
        CreateWeightedGraph(MediumGraphSize);
    }

    [Benchmark]
    public void CreateUnweightedGraph_Small()
    {
        CreateUnweightedGraph(SmallGraphSize);
    }

    [Benchmark]
    public void CreateUnweightedGraph_Medium()
    {
        CreateUnweightedGraph(MediumGraphSize);
    }

    #endregion

    #region Fluent API Benchmarks

    [Benchmark]
    public void FluentAPI_ShortestPath()
    {
        var source = "0";
        var target = (SmallGraphSize - 1).ToString();
        _smallWeightedGraph.ShortestPath().Dijkstra(_smallWeightedGraph).From(source, target);
    }

    [Benchmark]
    public void FluentAPI_MST()
    {
        _smallWeightedGraph.MinimumSpanningTree().Kruskal().Compute();
    }

    [Benchmark]
    public void FluentAPI_Coloring()
    {
        _smallUnweightedGraph.Coloring().Greedy().Optimal();
    }

    [Benchmark]
    public void FluentAPI_TopologicalSort()
    {
        _smallDirectedGraph.TopologicalSort().Kahn().Sort();
    }

    #endregion

    #region Helper Methods

    private WeightedUndirectedGraph<string, double> CreateWeightedGraph(int size)
    {
        var graph = new WeightedUndirectedGraph<string, double>();
        var random = new Random(42); // Fixed seed for reproducible results
        
        // Create a connected graph with random edges
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        // Create a spanning tree to ensure connectivity
        for (int i = 1; i < size; i++)
        {
            var parent = random.Next(i);
            var weight = random.NextDouble() * 10 + 1;
            graph.AddEdge(parent.ToString(), i.ToString(), weight);
        }
        
        // Add additional random edges
        var additionalEdges = size / 2;
        for (int i = 0; i < additionalEdges; i++)
        {
            var node1 = random.Next(size);
            var node2 = random.Next(size);
            if (node1 != node2)
            {
                var weight = random.NextDouble() * 10 + 1;
                graph.AddEdge(node1.ToString(), node2.ToString(), weight);
            }
        }
        
        return graph;
    }

    private UndirectedGraph<string> CreateUnweightedGraph(int size)
    {
        var graph = new UndirectedGraph<string>();
        var random = new Random(42);
        
        // Create a connected graph
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        // Create a spanning tree
        for (int i = 1; i < size; i++)
        {
            var parent = random.Next(i);
            graph.AddEdge(parent.ToString(), i.ToString());
        }
        
        // Add additional random edges
        var additionalEdges = size / 2;
        for (int i = 0; i < additionalEdges; i++)
        {
            var node1 = random.Next(size);
            var node2 = random.Next(size);
            if (node1 != node2)
            {
                graph.AddEdge(node1.ToString(), node2.ToString());
            }
        }
        
        return graph;
    }

    private DirectedGraph<string> CreateDirectedGraph(int size)
    {
        var graph = new DirectedGraph<string>();
        var random = new Random(42);
        
        // Create a DAG (no cycles)
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        // Add edges only from lower to higher numbered nodes to ensure DAG property
        for (int i = 0; i < size; i++)
        {
            var edgesFromNode = random.Next(3) + 1; // 1-3 edges per node
            for (int j = 0; j < edgesFromNode && i + j + 1 < size; j++)
            {
                var target = i + j + 1;
                if (target < size)
                {
                    graph.AddEdge(i.ToString(), target.ToString());
                }
            }
        }
        
        return graph;
    }

    #endregion
}
