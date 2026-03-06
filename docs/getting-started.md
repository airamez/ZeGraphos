# Getting Started with ZeGraphos

Welcome to ZeGraphos! This guide will help you get up and running with the library quickly.

## 📦 Installation

### NuGet Package

```bash
dotnet add package ZeGraphos
```

### Direct Reference

If you prefer to reference the library directly:

```xml
<ProjectReference Include="path/to/ZeGraphos.csproj" />
```

## 🎯 Basic Concepts

### Graph Types

ZeGraphos supports all combinations of graph types:

| | Unweighted | Weighted |
|---|---|---|
| **Directed** | `DirectedGraph<T>` | `WeightedDirectedGraph<T, TWeight>` |
| **Undirected** | `UndirectedGraph<T>` | `WeightedUndirectedGraph<T, TWeight>` |

### Generic Type Parameters

- **`T`**: The type of nodes in your graph (e.g., `string`, `int`, custom objects)
- **`TWeight`**: The type of edge weights (must be numeric, e.g., `double`, `int`, `decimal`)

## 🏗️ Creating Graphs

### Using GraphBuilder (Recommended)

```csharp
using ZeGraphos.Core.Common;
using ZeGraphos.Core.Implementations;

// Create different graph types
var directedGraph = GraphBuilder.CreateDirected<string>();
var undirectedGraph = GraphBuilder.CreateUndirected<int>();
var weightedGraph = GraphBuilder.CreateWeightedUndirected<string, double>();
var weightedDirectedGraph = GraphBuilder.CreateWeightedDirected<int, int>();
```

### Direct Instantiation

```csharp
var graph = new UndirectedGraph<string>();
var weightedGraph = new WeightedUndirectedGraph<string, double>();
```

## 🔧 Basic Operations

### Adding Nodes and Edges

```csharp
// Using GraphBuilder with fluent API
var graph = GraphBuilder.CreateUndirected<string>()
    .AddNodes("A", "B", "C", "D")
    .AddEdges(("A", "B"), ("B", "C"), ("C", "D"));

// Weighted edges
var weightedGraph = GraphBuilder.CreateWeightedUndirected<string, double>()
    .AddEdges(("A", "B", 2.5), ("B", "C", 1.8), ("C", "D", 3.2));

// Individual operations
var graph = new UndirectedGraph<string>();
graph.AddNode("A");
graph.AddEdge("A", "B");
```

### Common Graph Patterns

```csharp
// Complete graph
var completeGraph = GraphBuilder.CreateUndirected<string>();
completeGraph.MakeComplete("A", "B", "C", "D");

// Cycle graph
var cycleGraph = GraphBuilder.CreateUndirected<string>();
cycleGraph.MakeCycle("A", "B", "C", "D");

// Path graph
var pathGraph = GraphBuilder.CreateUndirected<string>();
pathGraph.MakePath("A", "B", "C", "D");

// Star graph
var starGraph = GraphBuilder.CreateUndirected<string>();
starGraph.MakeStar("Center", "Leaf1", "Leaf2", "Leaf3");
```

## 🌊 Fluent API

ZeGraphos provides a fluent API for algorithm discovery and chaining:

```csharp
using ZeGraphos.Extensions;

// Shortest path algorithms
var shortestPath = graph.ShortestPath()
    .Dijkstra(graph)
    .From("source", "target");

// Maximum flow algorithms
var maxFlow = weightedGraph.MaximumFlow()
    .EdmondsKarp()
    .From("source", "sink");

// Minimum spanning tree
var mst = weightedGraph.MinimumSpanningTree()
    .Kruskal()
    .Compute();

// Graph coloring
var coloring = graph.Coloring()
    .WelshPowell()
    .Compute();

// Topological sorting
var topoSort = directedGraph.TopologicalSort()
    .Kahn()
    .Sort();
```

## 📊 Algorithm Results

All algorithms return structured result objects:

```csharp
// Shortest path result
var pathResult = Dijkstra<string, double>.FindShortestPath(graph, "A", "D");
Console.WriteLine($"Path: {string.Join(" -> ", pathResult.Path)}");
Console.WriteLine($"Distance: {pathResult.Distance}");
Console.WriteLine($"Algorithm: {pathResult.Algorithm}");

// MST result
var mstResult = Kruskal<string, double>.ComputeMST(graph);
Console.WriteLine($"Total weight: {mstResult.TotalWeight}");
Console.WriteLine($"Edges: {mstResult.TreeEdges.Count}");
Console.WriteLine($"Spans all nodes: {mstResult.SpansAllNodes}");

// Coloring result
var coloringResult = GreedyColoring<string>.ColorGraph(graph);
Console.WriteLine($"Chromatic number: {coloringResult.ChromaticNumber}");
Console.WriteLine($"Valid coloring: {coloringResult.IsValid}");

// Topological sort result
var topoResult = Kahn<string>.Sort(directedGraph);
Console.WriteLine($"Is DAG: {topoResult.IsDAG}");
Console.WriteLine($"Sorted order: {string.Join(" -> ", topoResult.SortedOrder)}");
```

## 🔍 Querying Graphs

### Basic Properties

```csharp
var graph = GraphBuilder.CreateUndirected<string>()
    .AddEdges(("A", "B"), ("B", "C"));

Console.WriteLine($"Node count: {graph.NodeCount}");
Console.WriteLine($"Edge count: {graph.EdgeCount}");
Console.WriteLine($"Is directed: {graph.IsDirected}");
Console.WriteLine($"Is weighted: {graph.IsWeighted}");
```

### Node and Edge Operations

```csharp
// Check existence
bool hasNode = graph.ContainsNode("A");
bool hasEdge = graph.ContainsEdge("A", "B");

// Get neighbors
var neighbors = graph.GetNeighbors("A");

// Get degree
int degree = graph.GetDegree("A");

// Get all nodes and edges
var allNodes = graph.GetNodes();
var allEdges = graph.GetEdges();
```

### Weighted Graph Operations

```csharp
var weightedGraph = new WeightedUndirectedGraph<string, double>();
weightedGraph.AddEdge("A", "B", 2.5);

// Get edge weight
double weight = weightedGraph.GetEdgeWeight("A", "B");

// Set edge weight
weightedGraph.SetEdgeWeight("A", "B", 3.0);

// Try get edge weight
if (weightedGraph.TryGetEdgeWeight("A", "B", out var w))
{
    Console.WriteLine($"Weight: {w}");
}

// Get weighted edges and neighbors
var weightedEdges = weightedGraph.GetWeightedEdges();
var weightedNeighbors = weightedGraph.GetWeightedNeighbors("A");
```

## 🎨 Advanced Usage

### Custom Node Types

```csharp
public class City
{
    public string Name { get; set; }
    public int Population { get; set; }
    
    public override string ToString() => Name;
}

// Use custom objects as nodes
var cityGraph = new DirectedGraph<City>();
var cityA = new City { Name = "New York", Population = 8400000 };
var cityB = new City { Name = "Boston", Population = 675000 };

cityGraph.AddEdge(cityA, cityB);
```

### Algorithm Configuration

```csharp
// Graph coloring with different strategies
var greedyResult = graph.Coloring().Greedy()
    .WithOrdering(ColoringOrdering.DegreeDescending, 
                  ColoringStrategy.FirstAvailable);

var welshPowellResult = graph.Coloring().WelshPowell()
    .Optimized();

var dsaturResult = graph.Coloring().DSATUR()
    .Advanced(useDegreeTieBreaker: true);

// Topological sorting with deterministic ordering
var deterministicTopo = directedGraph.TopologicalSort()
    .Kahn()
    .Deterministic();
```

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "FullyQualifiedName~Algorithms"

# Run with detailed output
dotnet test --verbosity normal
```

### Test Structure

- **Core Tests**: Basic graph operations and data structures
- **Algorithm Tests**: Validation of all algorithms with known results
- **Performance Benchmarks**: Performance testing using BenchmarkDotNet

For detailed testing information, see the [Testing Guide](testing.md).

## 🎯 Next Steps

Now that you understand the basics, explore:

1. **[Algorithm Examples](examples/algorithm-examples.md)** - Detailed algorithm implementations
2. **[Real-world Scenarios](examples/real-world-scenarios.md)** - Practical applications
3. **[Performance Guide](performance.md)** - Optimization tips
4. **[API Reference](api/)** - Complete method documentation

## 💡 Tips

- Use `GraphBuilder` for cleaner, more readable code
- Leverage the fluent API for algorithm chaining
- Check algorithm result objects for additional metadata
- Use generic types that match your domain (e.g., `int` for IDs, custom objects for entities)
- Consider performance characteristics when choosing algorithms

Happy graph programming! 🎉
