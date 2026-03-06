using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ZeGraphos.Core.Implementations;
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Algorithms.Flow;
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Algorithms.TopologicalSort;
using ZeGraphos.Extensions;

namespace ZeGraphos.Tests.Performance;

/// <summary>
/// Specialized benchmarks for specific graph scenarios and edge cases.
/// Tests performance on dense graphs, sparse graphs, and special structures.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class ScenarioBenchmarks
{
    private const int GraphSize = 1000;
    
    // Special graph types
    private WeightedUndirectedGraph<string, double> _denseGraph = null!;
    private WeightedUndirectedGraph<string, double> _sparseGraph = null!;
    private UndirectedGraph<string> _completeGraph = null!;
    private UndirectedGraph<string> _starGraph = null!;
    private UndirectedGraph<string> _pathGraph = null!;
    private DirectedGraph<string> _linearDAG = null!;

    [GlobalSetup]
    public void Setup()
    {
        _denseGraph = CreateDenseGraph(GraphSize);
        _sparseGraph = CreateSparseGraph(GraphSize);
        _completeGraph = CreateCompleteGraph(100); // Smaller due to O(n²) edges
        _starGraph = CreateStarGraph(GraphSize);
        _pathGraph = CreatePathGraph(GraphSize);
        _linearDAG = CreateLinearDAG(GraphSize);
    }

    #region Density-Based Benchmarks

    [Benchmark]
    public void Dijkstra_DenseGraph()
    {
        Dijkstra<string, double>.FindShortestPath(_denseGraph, "0", (GraphSize - 1).ToString());
    }

    [Benchmark]
    public void Dijkstra_SparseGraph()
    {
        Dijkstra<string, double>.FindShortestPath(_sparseGraph, "0", (GraphSize - 1).ToString());
    }

    [Benchmark]
    public void Kruskal_DenseGraph()
    {
        Kruskal<string, double>.ComputeMST(_denseGraph);
    }

    [Benchmark]
    public void Kruskal_SparseGraph()
    {
        Kruskal<string, double>.ComputeMST(_sparseGraph);
    }

    #endregion

    #region Special Structure Benchmarks

    [Benchmark]
    public void Dijkstra_CompleteGraph()
    {
        Dijkstra<string, double>.FindShortestPath(
            CreateWeightedCompleteGraph(100), "0", "99");
    }

    [Benchmark]
    public void Prim_CompleteGraph()
    {
        Prim<string, double>.ComputeMST(CreateWeightedCompleteGraph(100));
    }

    [Benchmark]
    public void Coloring_CompleteGraph()
    {
        GreedyColoring<string>.ColorGraph(_completeGraph);
    }

    [Benchmark]
    public void Coloring_StarGraph()
    {
        GreedyColoring<string>.ColorGraph(_starGraph);
    }

    [Benchmark]
    public void Coloring_PathGraph()
    {
        GreedyColoring<string>.ColorGraph(_pathGraph);
    }

    [Benchmark]
    public void TopologicalSort_LinearDAG()
    {
        Kahn<string>.Sort(_linearDAG);
    }

    [Benchmark]
    public void DFS_TopologicalSort_LinearDAG()
    {
        DFSTopologicalSort<string>.Sort(_linearDAG);
    }

    #endregion

    #region Algorithm Comparison Benchmarks

    [Benchmark]
    public void Compare_MST_Algorithms_MediumGraph()
    {
        var graph = CreateWeightedGraph(500);
        
        Kruskal<string, double>.ComputeMST(graph);
        Prim<string, double>.ComputeMST(graph);
        Boruvka<string, double>.ComputeMST(graph);
    }

    [Benchmark]
    public void Compare_Coloring_Algorithms_MediumGraph()
    {
        var graph = CreateUnweightedGraph(500);
        
        GreedyColoring<string>.ColorGraph(graph);
        WelshPowell<string>.ColorGraph(graph);
        DSATUR<string>.ColorGraph(graph);
    }

    [Benchmark]
    public void Compare_TopologicalSort_Algorithms_MediumDAG()
    {
        var dag = CreateDirectedGraph(500);
        
        Kahn<string>.Sort(dag);
        DFSTopologicalSort<string>.Sort(dag);
    }

    #endregion

    #region Memory Allocation Benchmarks

    [Benchmark]
    public void GraphCreation_Weighted()
    {
        CreateWeightedGraph(100);
    }

    [Benchmark]
    public void GraphCreation_Unweighted()
    {
        CreateUnweightedGraph(100);
    }

    [Benchmark]
    public void GraphCreation_Directed()
    {
        CreateDirectedGraph(100);
    }

    #endregion

    #region Helper Methods

    private WeightedUndirectedGraph<string, double> CreateDenseGraph(int size)
    {
        var graph = new WeightedUndirectedGraph<string, double>();
        var random = new Random(42);
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        // Create a dense graph (approximately 30% of possible edges)
        var edgeCount = (int)(size * size * 0.3 / 2);
        for (int i = 0; i < edgeCount; i++)
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

    private WeightedUndirectedGraph<string, double> CreateSparseGraph(int size)
    {
        var graph = new WeightedUndirectedGraph<string, double>();
        var random = new Random(42);
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        // Create a sparse graph (approximately 2% of possible edges)
        var edgeCount = (int)(size * size * 0.02 / 2);
        for (int i = 0; i < edgeCount; i++)
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

    private UndirectedGraph<string> CreateCompleteGraph(int size)
    {
        var graph = new UndirectedGraph<string>();
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                graph.AddEdge(i.ToString(), j.ToString());
            }
        }
        
        return graph;
    }

    private WeightedUndirectedGraph<string, double> CreateWeightedCompleteGraph(int size)
    {
        var graph = new WeightedUndirectedGraph<string, double>();
        var random = new Random(42);
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                var weight = random.NextDouble() * 10 + 1;
                graph.AddEdge(i.ToString(), j.ToString(), weight);
            }
        }
        
        return graph;
    }

    private UndirectedGraph<string> CreateStarGraph(int size)
    {
        var graph = new UndirectedGraph<string>();
        graph.AddNode("center");
        
        for (int i = 1; i < size; i++)
        {
            graph.AddNode(i.ToString());
            graph.AddEdge("center", i.ToString());
        }
        
        return graph;
    }

    private UndirectedGraph<string> CreatePathGraph(int size)
    {
        var graph = new UndirectedGraph<string>();
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 0; i < size - 1; i++)
        {
            graph.AddEdge(i.ToString(), (i + 1).ToString());
        }
        
        return graph;
    }

    private DirectedGraph<string> CreateLinearDAG(int size)
    {
        var graph = new DirectedGraph<string>();
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 0; i < size - 1; i++)
        {
            graph.AddEdge(i.ToString(), (i + 1).ToString());
        }
        
        return graph;
    }

    // Reuse helper methods from AlgorithmBenchmarks
    private WeightedUndirectedGraph<string, double> CreateWeightedGraph(int size)
    {
        var graph = new WeightedUndirectedGraph<string, double>();
        var random = new Random(42);
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 1; i < size; i++)
        {
            var parent = random.Next(i);
            var weight = random.NextDouble() * 10 + 1;
            graph.AddEdge(parent.ToString(), i.ToString(), weight);
        }
        
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
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 1; i < size; i++)
        {
            var parent = random.Next(i);
            graph.AddEdge(parent.ToString(), i.ToString());
        }
        
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
        
        for (int i = 0; i < size; i++)
        {
            graph.AddNode(i.ToString());
        }
        
        for (int i = 0; i < size; i++)
        {
            var edgesFromNode = random.Next(3) + 1;
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
