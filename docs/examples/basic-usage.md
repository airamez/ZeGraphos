# Basic Usage Examples

This section provides practical examples of common graph operations and patterns using ZeGraphos.

## 🏗️ Creating Different Graph Types

### Simple Undirected Graph

```csharp
using ZeGraphos.Core.Implementations;
using ZeGraphos.Core.Common;

// Create a simple friendship network
var friendshipGraph = new UndirectedGraph<string>();
friendshipGraph.AddNodes("Alice", "Bob", "Charlie", "Diana");
friendshipGraph.AddEdges(("Alice", "Bob"), ("Bob", "Charlie"), ("Charlie", "Diana"));

Console.WriteLine($"Friends: {friendshipGraph.NodeCount}");
Console.WriteLine($"Friendships: {friendshipGraph.EdgeCount}");
```

### Weighted Graph - Distance Network

```csharp
using ZeGraphos.Core.Implementations;

// Create a city distance network
var distanceGraph = new WeightedUndirectedGraph<string, double>();
distanceGraph.AddEdge("New York", "Boston", 215.3);
distanceGraph.AddEdge("New York", "Philadelphia", 95.0);
distanceGraph.AddEdge("Boston", "Philadelphia", 306.0);

// Get distance between cities
double nyToBoston = distanceGraph.GetEdgeWeight("New York", "Boston");
Console.WriteLine($"NY to Boston: {nyToBoston} miles");
```

### Directed Graph - Workflow

```csharp
using ZeGraphos.Core.Implementations;

// Create a workflow dependency graph
var workflowGraph = new DirectedGraph<string>();
workflowGraph.AddEdges(
    ("Design", "Implementation"),
    ("Implementation", "Testing"),
    ("Testing", "Deployment"),
    ("Design", "Documentation")
);

// Check if one step depends on another
bool hasDependency = workflowGraph.ContainsEdge("Design", "Implementation");
Console.WriteLine($"Design depends on Implementation: {hasDependency}");
```

## 🔍 Graph Querying Examples

### Network Analysis

```csharp
// Analyze a social network
var socialGraph = GraphBuilder.CreateUndirected<string>()
    .AddNodes("Alice", "Bob", "Charlie", "Diana", "Eve")
    .AddEdges(("Alice", "Bob"), ("Alice", "Charlie"), ("Bob", "Diana"), 
             ("Charlie", "Diana"), ("Diana", "Eve"));

// Find Alice's friends
var aliceFriends = socialGraph.GetNeighbors("Alice");
Console.WriteLine($"Alice's friends: {string.Join(", ", aliceFriends)}");

// Find most connected person (highest degree)
var mostConnected = socialGraph.GetNodes()
    .OrderByDescending(node => socialGraph.GetDegree(node))
    .First();

Console.WriteLine($"Most connected: {mostConnected} with {socialGraph.GetDegree(mostConnected)} friends");
```

### Graph Properties

```csharp
var graph = GraphBuilder.CreateWeightedUndirected<string, int>()
    .AddEdges(("A", "B", 3), ("B", "C", 5), ("C", "D", 2));

// Basic properties
Console.WriteLine($"Nodes: {graph.NodeCount}");
Console.WriteLine($"Edges: {graph.EdgeCount}");
Console.WriteLine($"Directed: {graph.IsDirected}");
Console.WriteLine($"Weighted: {graph.IsWeighted}");

// Calculate total weight
var totalWeight = graph.GetWeightedEdges()
    .Sum(edge => edge.Weight);
Console.WriteLine($"Total weight: {totalWeight}");

// Check connectivity
var bfsResult = BFS<string>.FindConnectedComponents(graph);
Console.WriteLine($"Connected components: {bfsResult.Count}");
```

## 🌊 Algorithm Examples

### Shortest Path - Route Planning

```csharp
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Extensions;

// Create a road network
var roadNetwork = GraphBuilder.CreateWeightedUndirected<string, double>();
roadNetwork.AddEdges(
    ("A", "B", 10.0), ("A", "C", 15.0), ("B", "D", 12.0),
    ("C", "D", 10.0), ("D", "E", 5.0), ("B", "E", 15.0)
);

// Find shortest route from A to E
var route = roadNetwork.ShortestPath().Dijkstra(roadNetwork).From("A", "E");

Console.WriteLine($"Route: {string.Join(" -> ", route.Path)}");
Console.WriteLine($"Total distance: {route.Distance} km");
Console.WriteLine($"Algorithm: {route.Algorithm}");

// Alternative: BFS for unweighted shortest path
var unweightedRoute = BFS<string>.FindShortestPath(roadNetwork.ToUnweighted(), "A", "E");
Console.WriteLine($"Unweighted route: {string.Join(" -> ", unweightedRoute.Path)}");
```

### Minimum Spanning Tree - Network Design

```csharp
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Extensions;

// Create a network design problem
var network = GraphBuilder.CreateWeightedUndirected<string, int>();
network.AddEdges(
    ("A", "B", 4), ("A", "C", 3), ("B", "C", 1),
    ("B", "D", 2), ("C", "D", 4), ("C", "E", 2),
    ("D", "E", 3)
);

// Find minimum cost network
var mst = network.MinimumSpanningTree().Kruskal().Compute();

Console.WriteLine($"Minimum network cost: {mst.TotalWeight}");
Console.WriteLine($"Network edges:");
foreach (var edge in mst.TreeEdges)
{
    Console.WriteLine($"  {edge.Source} -- {edge.Target} (cost: {edge.Weight})");
}

// Compare with Prim's algorithm
var primMst = network.MinimumSpanningTree().Prim().Compute();
Console.WriteLine($"Prim's algorithm cost: {primMst.TotalWeight}");
```

### Graph Coloring - Schedule Planning

```csharp
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Extensions;

// Create a course scheduling problem (courses with conflicts)
var courseGraph = GraphBuilder.CreateUndirected<string>();
courseGraph.AddEdges(
    ("Math", "Physics"), // Time conflict
    ("Math", "Chemistry"), // Time conflict
    ("Physics", "Chemistry"), // Time conflict
    ("History", "Literature"), // Time conflict
    ("Art", "Music") // Time conflict
);

// Find minimum number of time slots
var schedule = courseGraph.Coloring().Greedy().Optimal();

Console.WriteLine($"Minimum time slots needed: {schedule.ChromaticNumber}");
Console.WriteLine($"Schedule:");
foreach (var color in schedule.GetColorGroups())
{
    var courses = schedule.GetNodesWithColor(color.Key);
    Console.WriteLine($"  Slot {color.Key}: {string.Join(", ", courses)}");
}

// Try different coloring strategies
var welshPowellSchedule = courseGraph.Coloring().WelshPowell().Compute();
Console.WriteLine($"Welsh-Powell slots: {welshPowellSchedule.ChromaticNumber}");
```

### Topological Sorting - Task Dependencies

```csharp
using ZeGraphos.Algorithms.TopologicalSort;
using ZeGraphos.Extensions;

// Create a task dependency graph
var tasks = GraphBuilder.CreateDirected<string>();
tasks.AddEdges(
    ("Requirements", "Design"),
    ("Design", "Implementation"),
    ("Implementation", "Testing"),
    ("Testing", "Deployment"),
    ("Requirements", "Documentation"),
    ("Design", "Documentation")
);

// Find execution order
var executionOrder = tasks.TopologicalSort().Kahn().Sort();

if (executionOrder.IsDAG)
{
    Console.WriteLine("Task execution order:");
    foreach (var (task, index) in executionOrder.SortedOrder.Select((task, i) => (task, i)))
    {
        Console.WriteLine($"  {index + 1}. {task}");
    }
}
else
{
    Console.WriteLine("Circular dependencies detected!");
    Console.WriteLine($"Unresolved tasks: {string.Join(", ", executionOrder.UnsortedNodes)}");
}

// Find tasks that can start immediately
var readyTasks = executionOrder.GetPredecessors("").ToList();
Console.WriteLine($"Tasks with no dependencies: {string.Join(", ", readyTasks)}");
```

## 🔧 Advanced Patterns

### Graph Transformation

```csharp
// Convert between graph types
var undirected = new UndirectedGraph<string>();
undirected.AddEdges(("A", "B"), ("B", "C"));

// Convert to directed
var directed = undirected.ToDirected();

// Convert to weighted
var weighted = undirected.ToWeighted<double>();
weighted.SetAllEdgeWeights(1.0);

// Copy graph
var copy = undirected.Copy();
```

### Custom Algorithms

```csharp
// Find all paths between two nodes (simple DFS)
public static List<List<string>> FindAllPaths<T>(IGraph<T> graph, T start, T end)
{
    var result = new List<List<string>>();
    var currentPath = new List<T>();
    var visited = new HashSet<T>();
    
    DFS(graph, start, end, currentPath, visited, result);
    return result;
}

private static void DFS<T>(IGraph<T> graph, T current, T end, 
    List<T> currentPath, HashSet<T> visited, List<List<T>> result)
{
    currentPath.Add(current);
    visited.Add(current);
    
    if (current.Equals(end))
    {
        result.Add(new List<T>(currentPath));
    }
    else
    {
        foreach (var neighbor in graph.GetNeighbors(current))
        {
            if (!visited.Contains(neighbor))
            {
                DFS(graph, neighbor, end, currentPath, visited, result);
            }
        }
    }
    
    currentPath.RemoveAt(currentPath.Count - 1);
    visited.Remove(current);
}

// Usage
var allPaths = FindAllPaths(friendshipGraph, "Alice", "Diana");
Console.WriteLine($"All paths from Alice to Diana: {allPaths.Count}");
foreach (var path in allPaths)
{
    Console.WriteLine($"  {string.Join(" -> ", path)}");
}
```

### Error Handling

```csharp
try
{
    // Try to find path in disconnected graph
    var path = Dijkstra<string, double>.FindShortestPath(graph, "A", "Z");
    
    if (path.Path.Count == 0)
    {
        Console.WriteLine("No path exists between A and Z");
    }
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"Node not found: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid graph for algorithm: {ex.Message}");
}

// Topological sort with cycle detection
try
{
    var topoResult = DFSTopologicalSort<string>.SortStrict(directedGraph);
    Console.WriteLine("Topological sort successful");
}
catch (CycleDetectedException<string> ex)
{
    Console.WriteLine($"Cycle detected: {string.Join(" -> ", ex.Cycle)}");
}
```

## 📊 Performance Tips

### Large Graphs

```csharp
// For large graphs, consider:
// 1. Using appropriate data structures
// 2. Choosing efficient algorithms
// 3. Monitoring memory usage

// Efficient node creation for large graphs
var largeGraph = new DirectedGraph<int>();
for (int i = 0; i < 10000; i++)
{
    largeGraph.AddNode(i);
}

// Batch edge addition
var edges = new List<(int, int)>();
for (int i = 0; i < 1000; i++)
{
    edges.Add((i, i + 1));
}

foreach (var (from, to) in edges)
{
    largeGraph.AddEdge(from, to);
}
```

### Algorithm Selection

```csharp
// Choose algorithms based on graph properties
if (graph.IsWeighted)
{
    // Use Dijkstra for weighted shortest path
    var path = Dijkstra<string, double>.FindShortestPath(graph, source, target);
}
else
{
    // Use BFS for unweighted shortest path
    var path = BFS<string>.FindShortestPath(graph, source, target);
}

// For dense graphs, Prim might be faster than Kruskal
if (graph.EdgeCount > graph.NodeCount * graph.NodeCount / 4)
{
    var mst = Prim<string, double>.ComputeMST(graph);
}
else
{
    var mst = Kruskal<string, double>.ComputeMST(graph);
}
```

These examples demonstrate the core functionality and common patterns when working with ZeGraphos. For more advanced scenarios, see the [real-world examples](real-world-scenarios.md) and [algorithm documentation](../algorithms.md).
