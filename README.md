# ZeGraphos

A comprehensive, well-designed graph library in C# .NET 10 that supports all common graph types with built-in algorithms and operations for single-machine memory usage.

## Features

- **Complete graph type implementations**: All combinations of weighted/non-weighted × directed/undirected
- **Interface-first API design**: Easy adoption with generic implementations
- **Common graph algorithms and operations**: Shortest path, maximum flow, minimum spanning tree, graph coloring, topological sorting
- **Comprehensive testing**: Unit tests and performance benchmarks
- **Single-machine memory optimization**: Efficient adjacency list implementation
- **NuGet package ready**: Ready for distribution and consumption

## Quick Start

```csharp
using ZeGraphos.Core.Common;
using ZeGraphos.Core.Implementations;
using ZeGraphos.Extensions;

// Create a directed graph
var graph = GraphBuilder.CreateDirected<string>()
    .AddNodes("A", "B", "C", "D")
    .AddEdges(("A", "B"), ("B", "C"), ("C", "D"));

// Find shortest path using BFS
var pathResult = graph.ShortestPath().BFS().From("A", "D");

// Create a weighted undirected graph
var weightedGraph = GraphBuilder.CreateWeightedUndirected<string, double>()
    .AddEdges(("A", "B", 2.5), ("B", "C", 1.8), ("C", "D", 3.2));

// Find shortest path using Dijkstra
var weightedPath = weightedGraph.ShortestPath().Dijkstra().From("A", "D");
```

## Installation

```
dotnet add package ZeGraphos
```

## Documentation

See the [docs](docs/) folder for detailed API documentation and examples.
