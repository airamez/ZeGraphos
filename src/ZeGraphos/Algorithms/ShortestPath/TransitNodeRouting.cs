using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.ShortestPath;

/// <summary>
/// Transit Node Routing algorithm for ultra-fast shortest path queries.
/// Uses precomputed access nodes and distances to achieve query times 
/// comparable to Contraction Hierarchies with different preprocessing approach.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public class TransitNodeRouting<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IConvertible, IEquatable<TWeight>
{
    private readonly IWeightedGraph<T, TWeight> _graph;
    private readonly Dictionary<T, HashSet<T>> _accessNodes;
    private readonly Dictionary<(T, T), TWeight> _transitDistances;
    private readonly Dictionary<T, Dictionary<T, TWeight>> _localDistances;
    private readonly List<T> _transitNodes;
    private readonly Func<TWeight, TWeight, TWeight> _add;
    private readonly Func<TWeight, TWeight, int> _compare;
    private readonly TWeight _infinity;

    public TransitNodeRouting(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _accessNodes = new Dictionary<T, HashSet<T>>();
        _transitDistances = new Dictionary<(T, T), TWeight>();
        _localDistances = new Dictionary<T, Dictionary<T, TWeight>>();
        _transitNodes = new List<T>();
        
        _add = ArithmeticOperations<TWeight>.Add;
        _compare = ArithmeticOperations<TWeight>.Compare;
        _infinity = ArithmeticOperations<TWeight>.GetMaxValue();

        Preprocess();
    }

    /// <summary>
    /// Finds the shortest path between two nodes using Transit Node Routing.
    /// </summary>
    public static PathResult<T, TWeight> FindShortestPath(
        IWeightedGraph<T, TWeight> graph, 
        T source, 
        T target)
    {
        var tnr = new TransitNodeRouting<T, TWeight>(graph);
        return tnr.FindPath(source, target);
    }

    /// <summary>
    /// Finds the shortest path using precomputed transit nodes.
    /// </summary>
    public PathResult<T, TWeight> FindPath(T source, T target)
    {
        if (!_graph.ContainsNode(source) || !_graph.ContainsNode(target))
            throw new KeyNotFoundException("Source or target node not found in graph");

        if (source.Equals(target))
            return new PathResult<T, TWeight>(source, source, default, new List<T> { source }, "Transit Node Routing", 0);

        // Check if direct path is shorter
        var directDistance = ComputeLocalDistance(source, target);
        var transitDistance = ComputeTransitDistance(source, target);

        if (_compare(directDistance, transitDistance) <= 0)
        {
            return ReconstructDirectPath(source, target, directDistance);
        }

        return ReconstructTransitPath(source, target, transitDistance);
    }

    private void Preprocess()
    {
        // Select transit nodes (simplified - in practice use more sophisticated selection)
        var allNodes = _graph.GetNodes().ToList();
        var nodeDegrees = allNodes.ToDictionary(n => n, n => _graph.GetDegree(n));
        
        // Select high-degree nodes as transit nodes (top 5%)
        var sortedNodes = allNodes.OrderByDescending(n => nodeDegrees[n]).ToList();
        var transitCount = Math.Max(1, allNodes.Count / 20);
        
        _transitNodes.AddRange(sortedNodes.Take(transitCount));

        // Compute access nodes and distances for all nodes
        foreach (var node in allNodes)
        {
            ComputeAccessNodes(node);
        }

        // Compute distances between all pairs of transit nodes
        ComputeTransitDistances();
    }

    private void ComputeAccessNodes(T node)
    {
        _accessNodes[node] = new HashSet<T>();
        _localDistances[node] = new Dictionary<T, TWeight>();

        // Run Dijkstra from node to find nearest transit nodes
        var distances = new Dictionary<T, TWeight>();
        var predecessors = new Dictionary<T, T>();
        var queue = new List<T> { node };
        distances[node] = default;

        while (queue.Any())
        {
            var current = queue.OrderBy(n => distances[n]).First();
            queue.Remove(current);

            // Stop if we've found enough transit nodes or reached too far
            if (_accessNodes[node].Count >= 5 || _compare(distances[current], _add(_add(_infinity, _infinity), _infinity)) > 0)
                break;

            // Check if current node is a transit node
            if (_transitNodes.Contains(current))
            {
                _accessNodes[node].Add(current);
                _localDistances[node][current] = distances[current];
            }

            // Continue search
            foreach (var neighbor in _graph.GetNeighbors(current))
            {
                var edgeWeight = _graph.GetEdgeWeight(current, neighbor);
                var newDistance = _add(distances[current], edgeWeight);

                if (!distances.ContainsKey(neighbor) || _compare(newDistance, distances[neighbor]) < 0)
                {
                    distances[neighbor] = newDistance;
                    predecessors[neighbor] = current;
                    queue.Add(neighbor);
                }
            }
        }
    }

    private void ComputeTransitDistances()
    {
        // Compute all-pairs shortest paths between transit nodes
        foreach (var source in _transitNodes)
        {
            var distances = Dijkstra<T, TWeight>.FindAllShortestPaths(_graph, source);
            
            foreach (var target in _transitNodes)
            {
                if (!source.Equals(target) && distances.Paths.ContainsKey(target))
                {
                    _transitDistances[(source, target)] = distances.GetPathTo(target)!.Distance;
                }
            }
        }
    }

    private TWeight ComputeLocalDistance(T source, T target)
    {
        // Use Dijkstra for local distance
        var result = Dijkstra<T, TWeight>.FindShortestPath(_graph, source, target);
        return result.Path.Any() ? result.Distance : _infinity;
    }

    private TWeight ComputeTransitDistance(T source, T target)
    {
        var minDistance = _infinity;

        // Try all combinations of access nodes
        foreach (var sourceAccess in _accessNodes[source])
        {
            foreach (var targetAccess in _accessNodes[target])
            {
                var totalDistance = _add(_add(_localDistances[source][sourceAccess], 
                    _transitDistances.TryGetValue((sourceAccess, targetAccess), out var td) ? td : _infinity),
                    _localDistances[target][targetAccess]);

                if (_compare(totalDistance, minDistance) < 0)
                {
                    minDistance = totalDistance;
                }
            }
        }

        return minDistance;
    }

    private PathResult<T, TWeight> ReconstructDirectPath(T source, T target, TWeight distance)
    {
        var result = Dijkstra<T, TWeight>.FindShortestPath(_graph, source, target);
        return new PathResult<T, TWeight>(source, target, distance, result.Path, "Transit Node Routing (Direct)", 0);
    }

    private PathResult<T, TWeight> ReconstructTransitPath(T source, T target, TWeight distance)
    {
        // Find best access node combination
        T bestSourceAccess = default!, bestTargetAccess = default!;
        var minDistance = _infinity;

        foreach (var sourceAccess in _accessNodes[source])
        {
            foreach (var targetAccess in _accessNodes[target])
            {
                var totalDistance = _add(_add(_localDistances[source][sourceAccess], 
                    _transitDistances.TryGetValue((sourceAccess, targetAccess), out var td) ? td : _infinity),
                    _localDistances[target][targetAccess]);

                if (_compare(totalDistance, minDistance) < 0)
                {
                    minDistance = totalDistance;
                    bestSourceAccess = sourceAccess;
                    bestTargetAccess = targetAccess;
                }
            }
        }

        // Reconstruct path: source -> sourceAccess -> targetAccess -> target
        if (bestSourceAccess == null || bestTargetAccess == null) return new PathResult<T, TWeight>(source, target, _infinity, new List<T>(), "Transit Node Routing", 0);
        var sourcePath = Dijkstra<T, TWeight>.FindShortestPath(_graph, source, bestSourceAccess);
        var transitPath = Dijkstra<T, TWeight>.FindShortestPath(_graph, bestSourceAccess, bestTargetAccess);
        var targetPath = Dijkstra<T, TWeight>.FindShortestPath(_graph, bestTargetAccess, target);

        var fullPath = new List<T>();
        fullPath.AddRange(sourcePath.Path);
        
        // Remove duplicate intermediate nodes
        if (sourcePath.Path.Count > 1 && transitPath.Path.Count > 1)
        {
            fullPath.RemoveAt(fullPath.Count - 1);
        }
        
        fullPath.AddRange(transitPath.Path);
        
        if (transitPath.Path.Count > 1 && targetPath.Path.Count > 1)
        {
            fullPath.RemoveAt(fullPath.Count - 1);
        }
        
        fullPath.AddRange(targetPath.Path);

        return new PathResult<T, TWeight>(source, target, distance, fullPath, "Transit Node Routing", 0);
    }

    /// <summary>
    /// Gets the access nodes for a given node.
    /// </summary>
    public IReadOnlyCollection<T> GetAccessNodes(T node)
    {
        return _accessNodes.TryGetValue(node, out var access) ? 
            new ReadOnlyCollection<T>(access.ToList()) : 
            new ReadOnlyCollection<T>(new List<T>());
    }

    /// <summary>
    /// Gets the transit nodes used in the routing.
    /// </summary>
    public IReadOnlyCollection<T> GetTransitNodes()
    {
        return new ReadOnlyCollection<T>(_transitNodes.ToList());
    }
}
