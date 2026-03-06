# Algorithms Guide

ZeGraphos provides a comprehensive collection of graph algorithms optimized for performance and ease of use.

## 🚀 Shortest Path Algorithms

### Dijkstra's Algorithm

**Best for**: Weighted graphs with non-negative edge weights

```csharp
using ZeGraphos.Algorithms.ShortestPath;

var graph = GraphBuilder.CreateWeightedUndirected<string, double>();
graph.AddEdges(("A", "B", 2.0), ("B", "C", 3.0), ("A", "C", 10.0));

var result = Dijkstra<string, double>.FindShortestPath(graph, "A", "C");
Console.WriteLine($"Path: {string.Join(" -> ", result.Path)}");
Console.WriteLine($"Distance: {result.Distance}");
```

**Characteristics**:
- ✅ Handles weighted graphs
- ✅ Guaranteed optimal path
- ✅ O(E + V log V) with priority queue
- ❌ Cannot handle negative weights

**Fluent API**:
```csharp
var result = graph.ShortestPath().Dijkstra(graph).From("A", "C");
```

### Breadth-First Search (BFS)

**Best for**: Unweighted graphs or when all edges have equal cost

```csharp
var result = BFS<string>.FindShortestPath(graph, "A", "C");
```

**Characteristics**:
- ✅ Unweighted graphs
- ✅ Guaranteed shortest path in terms of edges
- ✅ O(V + E) time complexity
- ✅ Can find connected components

**Additional BFS Features**:
```csharp
// Traversal order
var traversal = BFS<string>.Traverse(graph, "A");

// Connected components
var components = BFS<string>.FindConnectedComponents(graph);

// Connectivity check
bool connected = BFS<string>.AreConnected(graph, "A", "C");
```

### A* Search Algorithm

**Best for**: Weighted graphs with good heuristic functions

```csharp
var coords = new Dictionary<string, (int x, int y)>
{
    ["A"] = (0, 0), ["B"] = (1, 0), ["C"] = (0, 1)
};

var heuristic = AStar<string, double>.CreateManhattanHeuristic(coords);
var result = AStar<string, double>.FindShortestPath(graph, "A", "C", heuristic);
```

**Characteristics**:
- ✅ Weighted graphs with heuristics
- ✅ Often faster than Dijkstra with good heuristics
- ✅ Admissible heuristics guarantee optimality
- ❌ Requires domain-specific heuristic

**Built-in Heuristics**:
```csharp
var manhattan = AStar<string, double>.CreateManhattanHeuristic(coords);
var euclidean = AStar<string, double>.CreateEuclideanHeuristic(coords);
var zero = AStar<string, double>.CreateZeroHeuristic(); // Equivalent to Dijkstra
```

## 🌊 Maximum Flow Algorithms

### Ford-Fulkerson Algorithm

**Best for**: Educational purposes, simple flow networks

```csharp
var result = FordFulkerson<string, int>.ComputeMaxFlow(graph, "source", "sink");
Console.WriteLine($"Max flow: {result.MaxFlow}");
```

**Characteristics**:
- ✅ Simple implementation
- ✅ Finds maximum flow
- ❌ Can be slow on certain networks
- ❌ Performance depends on augmenting path selection

### Edmonds-Karp Algorithm

**Best for**: General purpose maximum flow, guaranteed performance

```csharp
var result = EdmondsKarp<string, int>.ComputeMaxFlow(graph, "source", "sink");
var (flow, cut) = graph.MaximumFlow().EdmondsKarp().FromWithCut("source", "sink");
```

**Characteristics**:
- ✅ O(VE²) worst-case time complexity
- ✅ Uses BFS for augmenting paths
- ✅ More predictable performance than Ford-Fulkerson
- ✅ Can find minimum cut

### Dinic's Algorithm

**Best for**: Large networks, performance-critical applications

```csharp
var result = Dinic<string, int>.ComputeMaxFlow(graph, "source", "sink");
```

**Characteristics**:
- ✅ O(E√V) for unit capacities, O(V²E) general
- ✅ Level graph + blocking flow approach
- ✅ Often fastest in practice
- ✅ Scales well to large networks

**Flow Result Features**:
```csharp
Console.WriteLine($"Max flow: {result.MaxFlow}");
Console.WriteLine($"Algorithm: {result.Algorithm}");
Console.WriteLine($"Iterations: {result.Iterations}");
Console.WriteLine($"Flow edges: {result.FlowEdges.Count}");
```

## 🌳 Minimum Spanning Tree Algorithms

### Kruskal's Algorithm

**Best for**: Sparse graphs, when edges can be easily sorted

```csharp
var result = Kruskal<string, double>.ComputeMST(graph);
Console.WriteLine($"Total weight: {result.TotalWeight}");
```

**Characteristics**:
- ✅ O(E log E) time complexity
- ✅ Good for sparse graphs
- ✅ Uses Union-Find data structure
- ✅ Can handle disconnected graphs (forest)

**Forest Support**:
```csharp
var forest = Kruskal<string, double>.ComputeMinimumForest(graph);
Console.WriteLine($"Number of trees: {forest.TreeCount}");
```

### Prim's Algorithm

**Best for**: Dense graphs, when graph is represented as adjacency list

```csharp
var result = Prim<string, double>.ComputeMST(graph, "startNode");
```

**Characteristics**:
- ✅ O(E log V) with binary heap
- ✅ Good for dense graphs
- ✅ Grows MST from starting node
- ✅ Can handle disconnected graphs

### Borůvka's Algorithm

**Best for**: Parallel processing, very large graphs

```csharp
var result = Boruvka<string, double>.ComputeMST(graph);
```

**Characteristics**:
- ✅ O(E log V) time complexity
- ✅ Naturally parallelizable
- ✅ Good for distributed computing
- ✅ Multiple components processed simultaneously

**MST Result Features**:
```csharp
Console.WriteLine($"Total weight: {result.TotalWeight}");
Console.WriteLine($"Spans all nodes: {result.SpansAllNodes}");
Console.WriteLine($"Number of edges: {result.TreeEdges.Count}");
Console.WriteLine($"Algorithm: {result.Algorithm}");
```

## 🎨 Graph Coloring Algorithms

### Greedy Coloring

**Best for**: Fast approximation, when exact solution isn't critical

```csharp
var result = GreedyColoring<string>.ColorGraph(graph, 
    ColoringOrdering.DegreeDescending, 
    ColoringStrategy.FirstAvailable);
```

**Ordering Strategies**:
- `Natural`: Nodes in insertion order
- `DegreeDescending`: High degree nodes first
- `DegreeAscending`: Low degree nodes first
- `Random`: Random order

**Coloring Strategies**:
- `FirstAvailable`: First available color
- `LeastUsed`: Color used least frequently
- `MostUsed`: Color used most frequently

**Optimization**:
```csharp
var improved = GreedyColoring<string>.ImproveColoring(graph, initialResult);
```

### Welsh-Powell Algorithm

**Best for**: Better approximation than basic greedy

```csharp
var result = WelshPowell<string>.ColorGraph(graph);
var optimized = WelshPowell<string>.ColorGraphOptimized(graph, useOptimization: true);
```

**Characteristics**:
- ✅ Orders nodes by degree
- ✅ Better than simple greedy
- ✅ O(V²) time complexity
- ✅ Provides chromatic number bounds

### DSATUR Algorithm

**Best for**: Best approximation quality among greedy methods

```csharp
var result = DSATUR<string>.ColorGraph(graph);
var advanced = DSATUR<string>.ColorGraphAdvanced(graph, 
    useDegreeTieBreaker: true, 
    useRandomTieBreaker: false);
```

**Characteristics**:
- ✅ Uses saturation degree
- ✅ Often gives best greedy results
- ✅ O(V³) worst-case time
- ✅ Adaptive node selection

**Coloring Result Features**:
```csharp
Console.WriteLine($"Chromatic number: {result.ChromaticNumber}");
Console.WriteLine($"Valid coloring: {result.IsValid}");
Console.WriteLine($"Color groups: {result.GetColorGroups().Count}");

// Get nodes with specific color
var redNodes = result.GetNodesWithColor(1);

// Check if two nodes have same color
bool sameColor = result.HaveSameColor("A", "B");
```

## 🔄 Topological Sorting Algorithms

### Kahn's Algorithm

**Best for**: General purpose topological sorting

```csharp
var result = Kahn<string>.Sort(graph);
var deterministic = Kahn<string>.SortDeterministic(graph, comparer);
```

**Characteristics**:
- ✅ O(V + E) time complexity
- ✅ Uses in-degree counting
- ✅ Detects cycles
- ✅ Can be made deterministic

**Additional Features**:
```csharp
// Find source and sink nodes
var sources = Kahn<string>.FindSourceNodes(graph);
var sinks = Kahn<string>.FindSinkNodes(graph);

// Check for cycles
bool hasCycle = Kahn<string>.HasCycle(graph);
```

### DFS-Based Topological Sort

**Best for**: When you need cycle detection information

```csharp
var result = DFSTopologicalSort<string>.Sort(graph);
var strict = DFSTopologicalSort<string>.SortStrict(graph); // Throws on cycles
```

**Characteristics**:
- ✅ O(V + E) time complexity
- ✅ Uses depth-first search
- ✅ Can find actual cycles
- ✅ Reverse post-order processing

**Cycle Detection**:
```csharp
if (result.IsDAG)
{
    Console.WriteLine("Topological order: {string.Join(" -> ", result.SortedOrder)}");
}
else
{
    Console.WriteLine("Cycle detected!");
    var cycle = DFSTopologicalSort<string>.FindCycle(graph);
    Console.WriteLine($"Cycle: {string.Join(" -> ", cycle)}");
}
```

**Topological Sort Result Features**:
```csharp
Console.WriteLine($"Is DAG: {result.IsDAG}");
Console.WriteLine($"Is complete: {result.IsComplete}");
Console.WriteLine($"Sorted nodes: {result.SortedOrder.Count}");
Console.WriteLine($"Unsorted nodes: {result.UnsortedNodes.Count}");

// Get position of node in order
int position = result.GetPosition("A");

// Get predecessors and successors
var preds = result.GetPredecessors("A");
var succs = result.GetSuccessors("A");

// Check ordering
bool isBefore = result.IsBefore("A", "B");
```

## 📊 Algorithm Comparison

### Performance Characteristics

| Algorithm | Time Complexity | Space Complexity | Best For |
|-----------|------------------|------------------|----------|
| Dijkstra | O(E + V log V) | O(V) | Weighted graphs |
| BFS | O(V + E) | O(V) | Unweighted graphs |
| A* | Depends on heuristic | O(V) | Weighted + heuristic |
| Ford-Fulkerson | O(max_flow × E) | O(V + E) | Simple networks |
| Edmonds-Karp | O(VE²) | O(V + E) | General purpose |
| Dinic | O(V²E) | O(V + E) | Large networks |
| Kruskal | O(E log E) | O(V + E) | Sparse graphs |
| Prim | O(E log V) | O(V + E) | Dense graphs |
| Borůvka | O(E log V) | O(V + E) | Parallel processing |
| Greedy Coloring | O(V²) | O(V) | Fast approximation |
| Welsh-Powell | O(V²) | O(V) | Better approximation |
| DSATUR | O(V³) | O(V) | Best greedy |
| Kahn | O(V + E) | O(V) | General purpose |
| DFS Topo | O(V + E) | O(V) | Cycle detection |

### Choosing the Right Algorithm

**Shortest Path**:
- Unweighted graph → BFS
- Weighted graph, positive weights → Dijkstra
- Weighted graph + good heuristic → A*
- Need all pairs → Run algorithm from each source

**Maximum Flow**:
- Simple network → Ford-Fulkerson
- General purpose → Edmonds-Karp
- Large network → Dinic
- Need minimum cut → Edmonds-Karp or Dinic

**Minimum Spanning Tree**:
- Sparse graph → Kruskal
- Dense graph → Prim
- Parallel processing → Borůvka
- Disconnected graph → Any with forest support

**Graph Coloring**:
- Fast approximation → Greedy
- Better approximation → Welsh-Powell
- Best greedy → DSATUR
- Exact solution needed → Consider specialized algorithms

**Topological Sort**:
- General purpose → Kahn
- Need cycle information → DFS-based
- Deterministic order needed → Kahn with comparator

## 🔧 Advanced Usage

### Algorithm Chaining

```csharp
// Chain multiple operations
var result = graph
    .ShortestPath()
    .Dijkstra(graph)
    .From("source", "target");

var mst = graph
    .MinimumSpanningTree()
    .Kruskal()
    .Compute();

var coloring = graph
    .Coloring()
    .DSATUR()
    .Advanced();
```

### Custom Algorithm Configuration

```csharp
// Custom heuristic for A*
Func<string, string, double> customHeuristic = (from, to) => 
{
    // Your custom logic here
    return CalculateDistance(from, to);
};

var result = AStar<string, double>.FindShortestPath(graph, from, to, customHeuristic);
```

### Error Handling

```csharp
try
{
    var result = DFSTopologicalSort<string>.SortStrict(graph);
}
catch (CycleDetectedException<string> ex)
{
    Console.WriteLine($"Cycle detected: {string.Join(" -> ", ex.Cycle)}");
}
```

This guide covers the core algorithms in ZeGraphos. For more detailed examples, see the [Examples](examples/) section.
