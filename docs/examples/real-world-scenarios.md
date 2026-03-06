# Real-World Scenarios

This section demonstrates practical applications of ZeGraphos in real-world scenarios.

## 🌐 Network Routing

### Internet Router Network

```csharp
using ZeGraphos.Core.Implementations;
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Extensions;

// Model internet backbone with routers and connection costs
var internetBackbone = GraphBuilder.CreateWeightedUndirected<string, double>();

// Add major routers and connections with latency (ms)
internetBackbone.AddEdges(
    ("NYC", "Boston", 5.2),
    ("NYC", "Washington", 8.1),
    ("NYC", "Chicago", 15.3),
    ("Chicago", "Denver", 12.7),
    ("Denver", "SanFrancisco", 18.9),
    ("Chicago", "Atlanta", 11.4),
    ("Atlanta", "Dallas", 9.8),
    ("Dallas", "LosAngeles", 14.2),
    ("SanFrancisco", "LosAngeles", 6.3),
    ("LosAngeles", "Seattle", 16.7)
);

// Find optimal routing path
var route = internetBackbone.ShortestPath().Dijkstra(internetBackbone).From("NYC", "LosAngeles");

Console.WriteLine($"Optimal route NYC -> LA: {string.Join(" -> ", route.Path)}");
Console.WriteLine($"Total latency: {route.Distance} ms");

// Find all paths from NYC for redundancy analysis
var allPathsFromNYC = Dijkstra<string, double>.FindAllShortestPaths(internetBackbone, "NYC");
Console.WriteLine("Reachable cities from NYC:");
foreach (var city in allPathsFromNYC.Paths.Keys)
{
    Console.WriteLine($"  {city}: {allPathsFromNYC.GetPath(city).Distance} ms");
}
```

### Flight Route Optimization

```csharp
// Airport network with flight times
var flightNetwork = GraphBuilder.CreateWeightedUndirected<string, double>();
flightNetwork.AddEdges(
    ("JFK", "LAX", 5.5), // 5.5 hours
    ("JFK", "ORD", 2.5), // Chicago
    ("JFK", "ATL", 2.0), // Atlanta
    ("LAX", "SFO", 1.2), // San Francisco
    ("ORD", "DEN", 2.3), // Denver
    ("ATL", "MIA", 2.0), // Miami
    ("DEN", "SFO", 2.8),
    ("SFO", "SEA", 2.1)  // Seattle
);

// Find fastest route from JFK to SFO
var flightRoute = flightNetwork.ShortestPath().Dijkstra(flightNetwork).From("JFK", "SFO");
Console.WriteLine($"Fastest route: {string.Join(" -> ", flightRoute.Path)}");
Console.WriteLine($"Flight time: {flightRoute.Distance} hours");

// Calculate network connectivity
var connectivity = BFS<string>.FindConnectedComponents(flightNetwork);
Console.WriteLine($"Network has {connectivity.Count} connected component(s)");
```

## 🏗️ Project Management

### Task Dependency Management

```csharp
using ZeGraphos.Algorithms.TopologicalSort;
using ZeGraphos.Extensions;

// Software project task dependencies
var projectTasks = GraphBuilder.CreateDirected<string>();
projectTasks.AddEdges(
    ("Requirements", "Architecture"),
    ("Requirements", "UI_Design"),
    ("Architecture", "Backend_Dev"),
    ("UI_Design", "Frontend_Dev"),
    ("Backend_Dev", "API_Documentation"),
    ("Frontend_Dev", "UI_Testing"),
    ("Backend_Dev", "Backend_Testing"),
    ("Backend_Testing", "Integration_Testing"),
    ("UI_Testing", "Integration_Testing"),
    ("Integration_Testing", "Deployment"),
    ("API_Documentation", "Deployment")
);

// Find optimal task execution order
var taskOrder = projectTasks.TopologicalSort().Kahn().Sort();

if (taskOrder.IsDAG)
{
    Console.WriteLine("Optimal task execution order:");
    foreach (var (task, index) in taskOrder.SortedOrder.Select((task, i) => (task, i)))
    {
        var dependencies = taskOrder.GetPredecessors(task);
        var depStr = dependencies.Any() ? $" (depends on: {string.Join(", ", dependencies)})" : "";
        Console.WriteLine($"  {index + 1}. {task}{depStr}");
    }
    
    // Find critical path (longest dependency chain)
    var criticalPath = FindCriticalPath(projectTasks, taskOrder);
    Console.WriteLine($"\nCritical path: {string.Join(" -> ", criticalPath)}");
}
else
{
    Console.WriteLine("Circular dependencies detected!");
    Console.WriteLine($"Conflicting tasks: {string.Join(", ", taskOrder.UnsortedNodes)}");
}

// Helper method to find critical path
List<string> FindCriticalPath(DirectedGraph<string> tasks, TopologicalSortResult<string> order)
{
    var longestPaths = new Dictionary<string, int>();
    var predecessors = new Dictionary<string, List<string>>();
    
    // Initialize paths
    foreach (var task in order.SortedOrder)
    {
        longestPaths[task] = 0;
        predecessors[task] = order.GetPredecessors(task).ToList();
    }
    
    // Calculate longest paths
    foreach (var task in order.SortedOrder)
    {
        foreach (var pred in predecessors[task])
        {
            if (longestPaths[pred] + 1 > longestPaths[task])
            {
                longestPaths[task] = longestPaths[pred] + 1;
            }
        }
    }
    
    // Reconstruct critical path
    var endTask = longestPaths.OrderByDescending(kvp => kvp.Value).First().Key;
    var criticalPath = new List<string> { endTask };
    
    while (predecessors[endTask].Any())
    {
        var nextPred = predecessors[endTask]
            .OrderByDescending(p => longestPaths[p])
            .First();
        criticalPath.Insert(0, nextPred);
        endTask = nextPred;
    }
    
    return criticalPath;
}
```

### Resource Allocation

```csharp
using ZeGraphos.Algorithms.Coloring;
using ZeGraphos.Extensions;

// Meeting room scheduling - meetings that conflict in time
var meetingSchedules = GraphBuilder.CreateUndirected<string>();
meetingSchedules.AddEdges(
    ("Team_Meeting", "Client_Call"), // Same time
    ("Team_Meeting", "Code_Review"), // Same time
    ("Client_Call", "Design_Review"), // Same time
    ("Code_Review", "Design_Review"), // Same time
    ("Project_Plan", "Budget_Review"), // Same time
    ("Budget_Review", "Q4_Review") // Same time
);

// Find minimum number of meeting rooms needed
var roomAssignment = meetingSchedules.Coloring().Greedy().Optimal();

Console.WriteLine($"Minimum meeting rooms needed: {roomAssignment.ChromaticNumber}");
Console.WriteLine("Room assignments:");
foreach (var room in roomAssignment.GetColorGroups())
{
    var meetings = roomAssignment.GetNodesWithColor(room.Key);
    Console.WriteLine($"  Room {room.Key}: {string.Join(", ", meetings)}");
}

// Check if a specific time slot is available
bool isTimeSlotAvailable = !roomAssignment.GetNodesWithColor(1).Contains("Team_Meeting");
Console.WriteLine($"Room 1 available for Team_Meeting: {isTimeSlotAvailable}");
```

## 🎮 Game Development

### Navigation Mesh for AI

```csharp
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Extensions;

// Game world navigation points with movement costs
var navMesh = GraphBuilder.CreateWeightedUndirected<string, double>();

// Add waypoints and connections (costs represent difficulty/distance)
navMesh.AddEdges(
    ("Spawn", "Corridor", 1.0),
    ("Corridor", "Room1", 2.0),
    ("Corridor", "Room2", 1.5),
    ("Room1", "Treasure", 3.0),
    ("Room2", "Treasure", 2.5),
    ("Treasure", "Exit", 4.0),
    ("Room1", "Secret", 5.0),
    ("Secret", "Exit", 2.0)
);

// AI pathfinding from spawn to exit
var aiPath = navMesh.ShortestPath().Dijkstra(navMesh).From("Spawn", "Exit");

Console.WriteLine($"AI path: {string.Join(" -> ", aiPath.Path)}");
Console.WriteLine($"Path cost: {aiPath.Distance}");

// Find alternative path if main path is blocked
var alternativePath = navMesh.ShortestPath().Dijkstra(navMesh).From("Spawn", "Exit");
Console.WriteLine($"Alternative path cost: {alternativePath.Distance}");

// Calculate all distances from player position for AI decision making
var playerPos = "Room1";
var distancesFromPlayer = Dijkstra<string, double>.FindAllShortestPaths(navMesh, playerPos);

Console.WriteLine("Distances from player:");
foreach (var location in distancesFromPlayer.Paths.Keys)
{
    var distance = distancesFromPlayer.GetPath(location).Distance;
    Console.WriteLine($"  {location}: {distance}");
}
```

### Game Economy - Trade Routes

```csharp
// Trading game - cities with trade route costs
var tradeNetwork = GraphBuilder.CreateWeightedUndirected<string, double>();
tradeNetwork.AddEdges(
    ("City_A", "City_B", 10.0), // Trade cost
    ("City_A", "City_C", 15.0),
    ("City_B", "City_D", 8.0),
    ("City_C", "City_D", 12.0),
    ("City_C", "City_E", 20.0),
    ("City_D", "City_E", 6.0)
);

// Find cheapest trade route
var tradeRoute = tradeNetwork.ShortestPath().Dijkstra(tradeNetwork).From("City_A", "City_E");
Console.WriteLine($"Cheapest trade route: {string.Join(" -> ", tradeRoute.Path)}");
Console.WriteLine($"Total cost: {tradeRoute.Distance}");

// MST for building minimum cost road network
var roadNetwork = tradeNetwork.MinimumSpanningTree().Kruskal().Compute();
Console.WriteLine($"Minimum road network cost: {roadNetwork.TotalWeight}");
Console.WriteLine("Road network connections:");
foreach (var road in roadNetwork.TreeEdges)
{
    Console.WriteLine($"  {road.Source} -- {road.Target} (cost: {road.Weight})");
}
```

## 📊 Data Analysis

### Social Network Analysis

```csharp
using ZeGraphos.Algorithms.ShortestPath;
using ZeGraphos.Extensions;

// Social network analysis
var socialNetwork = GraphBuilder.CreateUndirected<string>();
socialNetwork.AddEdges(
    ("Alice", "Bob"), ("Alice", "Charlie"), ("Alice", "David"),
    ("Bob", "Eve"), ("Bob", "Frank"),
    ("Charlie", "Grace"), ("Charlie", "Henry"),
    ("David", "Ivy"), ("David", "Jack"),
    ("Eve", "Frank"), ("Grace", "Henry"), ("Ivy", "Jack")
);

// Find most influential person (highest betweenness centrality approximation)
var mostInfluential = FindMostInfluential(socialNetwork);
Console.WriteLine($"Most influential person: {mostInfluential}");

// Find shortest paths between all pairs
var allShortestPaths = new Dictionary<(string, string), List<string>>();
foreach (var person1 in socialNetwork.GetNodes())
{
    foreach (var person2 in socialNetwork.GetNodes())
    {
        if (person1 != person2)
        {
            var path = BFS<string>.FindShortestPath(socialNetwork, person1, person2);
            if (path.Path.Any())
            {
                allShortestPaths[(person1, person2)] = path.Path;
            }
        }
    }
}

// Find average path length (network cohesion)
var pathLengths = allShortestPaths.Values.Select(path => path.Count - 1).ToList();
var avgPathLength = pathLengths.Average();
Console.WriteLine($"Average path length: {avgPathLength:F2}");

// Find communities using connected components
var communities = BFS<string>.FindConnectedComponents(socialNetwork);
Console.WriteLine($"Number of communities: {communities.Count}");
foreach (var (community, index) in communities.Select((c, i) => (c, i)))
{
    Console.WriteLine($"  Community {index + 1}: {string.Join(", ", community)}");
}

string FindMostInfluential(UndirectedGraph<string> network)
{
    var betweenness = new Dictionary<string, int>();
    
    // Simple betweenness centrality approximation
    foreach (var node in network.GetNodes())
    {
        betweenness[node] = 0;
    }
    
    foreach (var source in network.GetNodes())
    {
        foreach (var target in network.GetNodes())
        {
            if (source != target)
            {
                var path = BFS<string>.FindShortestPath(network, source, target);
                if (path.Path.Count > 2)
                {
                    // Count intermediate nodes in shortest paths
                    for (int i = 1; i < path.Path.Count - 1; i++)
                    {
                        betweenness[path.Path[i]]++;
                    }
                }
            }
        }
    }
    
    return betweenness.OrderByDescending(kvp => kvp.Value).First().Key;
}
```

### Supply Chain Optimization

```csharp
using ZeGraphos.Algorithms.SpanningTree;
using ZeGraphos.Extensions;

// Supply chain network with transportation costs
var supplyChain = GraphBuilder.CreateWeightedUndirected<string, double>();
supplyChain.AddEdges(
    ("Factory_A", "Warehouse_1", 100.0),
    ("Factory_A", "Warehouse_2", 150.0),
    ("Factory_B", "Warehouse_2", 80.0),
    ("Factory_B", "Warehouse_3", 120.0),
    ("Warehouse_1", "Store_1", 50.0),
    ("Warehouse_1", "Store_2", 60.0),
    ("Warehouse_2", "Store_2", 40.0),
    ("Warehouse_2", "Store_3", 55.0),
    ("Warehouse_3", "Store_3", 45.0),
    ("Warehouse_3", "Store_4", 70.0)
);

// Find minimum cost distribution network
var distributionNetwork = supplyChain.MinimumSpanningTree().Kruskal().Compute();
Console.WriteLine($"Minimum distribution network cost: {distributionNetwork.TotalWeight}");

// Identify bottlenecks (edges with high weight in MST)
var bottlenecks = distributionNetwork.TreeEdges
    .OrderByDescending(edge => edge.Weight)
    .Take(3);

Console.WriteLine("Potential bottlenecks:");
foreach (var bottleneck in bottlenecks)
{
    Console.WriteLine($"  {bottleneck.Source} -- {bottleneck.Target} (cost: {bottleneck.Weight})");
}
```

## 🔬 Scientific Computing

### Molecular Structure Analysis

```csharp
// Molecular graph - atoms as nodes, bonds as edges
var molecule = GraphBuilder.CreateUndirected<string>();
molecule.AddEdges(
    ("C1", "C2"), ("C2", "C3"), ("C3", "C4"), ("C4", "C5"), ("C5", "C6"), ("C6", "C1"), // Benzene ring
    ("C1", "H1"), ("C2", "H2"), ("C3", "H3"), ("C4", "H4"), ("C5", "H5"), ("C6", "H6") // Hydrogen atoms
);

// Find molecular properties
var connectivity = BFS<string>.FindConnectedComponents(molecule);
Console.WriteLine($"Molecule has {connectivity.Count} connected component(s)");

// Find all cycles in the molecule (ring detection)
var cycles = FindCycles(molecule);
Console.WriteLine($"Number of rings: {cycles.Count}");
foreach (var (cycle, index) in cycles.Select((c, i) => (c, i)))
{
    Console.WriteLine($"  Ring {index + 1}: {string.Join(" -> ", cycle)}");
}

List<List<string>> FindCycles(UndirectedGraph<string> graph)
{
    var cycles = new List<List<string>>();
    var visited = new HashSet<string>();
    
    foreach (var node in graph.GetNodes())
    {
        if (!visited.Contains(node))
        {
            var cycle = new List<string>();
            FindCyclesDFS(graph, node, node, new HashSet<string>(), cycle, cycles, visited);
        }
    }
    
    return cycles;
}

void FindCyclesDFS(UndirectedGraph<string> graph, string current, string start, 
    HashSet<string> pathSet, List<string> currentPath, 
    List<List<string>> cycles, HashSet<string> globalVisited)
{
    currentPath.Add(current);
    pathSet.Add(current);
    
    foreach (var neighbor in graph.GetNeighbors(current))
    {
        if (neighbor == start && currentPath.Count > 2)
        {
            // Found a cycle
            cycles.Add(new List<string>(currentPath));
        }
        else if (!pathSet.Contains(neighbor))
        {
            FindCyclesDFS(graph, neighbor, start, pathSet, currentPath, cycles, globalVisited);
        }
    }
    
    currentPath.RemoveAt(currentPath.Count - 1);
    pathSet.Remove(current);
    globalVisited.Add(current);
}
```

These real-world examples demonstrate how ZeGraphos can be applied to solve practical problems across various domains. The library's flexibility and comprehensive algorithm support make it suitable for a wide range of applications.
