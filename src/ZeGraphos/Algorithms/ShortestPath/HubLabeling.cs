using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZeGraphos.Core.Interfaces;
using ZeGraphos.Core.Common;

namespace ZeGraphos.Algorithms.ShortestPath;

/// <summary>
/// Hub Labeling algorithm for ultra-fast point-to-point shortest path queries.
/// Uses precomputed labels (hubs and distances) to achieve query times 
/// in microseconds while maintaining exact shortest paths.
/// </summary>
/// <typeparam name="T">The type of nodes in the graph</typeparam>
/// <typeparam name="TWeight">The type of edge weights</typeparam>
public class HubLabeling<T, TWeight>
    where T : notnull
    where TWeight : struct, IComparable<TWeight>, IConvertible, IEquatable<TWeight>
{
    private readonly IWeightedGraph<T, TWeight> _graph;
    private readonly Dictionary<T, List<(T Hub, TWeight Distance)>> _forwardLabels;
    private readonly Dictionary<T, List<(T Hub, TWeight Distance)>> _backwardLabels;
    private List<T> _nodesOrdered = null!;
    private readonly Func<TWeight, TWeight, TWeight> _add;
    private readonly Func<TWeight, TWeight, int> _compare;
    private readonly TWeight _infinity;

    public HubLabeling(IWeightedGraph<T, TWeight> graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _forwardLabels = new Dictionary<T, List<(T, TWeight)>>();
        _backwardLabels = new Dictionary<T, List<(T, TWeight)>>();
        
        _add = ArithmeticOperations<TWeight>.Add;
        _compare = ArithmeticOperations<TWeight>.Compare;
        _infinity = ArithmeticOperations<TWeight>.GetMaxValue();

        Preprocess();
    }

    /// <summary>
    /// Finds the shortest path between two nodes using Hub Labeling.
    /// </summary>
    public static PathResult<T, TWeight> FindShortestPath(
        IWeightedGraph<T, TWeight> graph, 
        T source, 
        T target)
    {
        var hl = new HubLabeling<T, TWeight>(graph);
        return hl.FindPath(source, target);
    }

    /// <summary>
    /// Finds the shortest path using precomputed hub labels.
    /// </summary>
    public PathResult<T, TWeight> FindPath(T source, T target)
    {
        if (!_graph.ContainsNode(source) || !_graph.ContainsNode(target))
            throw new KeyNotFoundException("Source or target node not found in graph");

        if (source.Equals(target))
            return new PathResult<T, TWeight>(source, source, default, new List<T> { source }, "Hub Labeling", 0);

        // Query using hub labels
        var (distance, meetingHub) = QueryDistance(source, target);
        
        if (_compare(distance, _infinity) == 0)
        {
            return new PathResult<T, TWeight>(source, target, _infinity, new List<T>(), "Hub Labeling", 0);
        }

        return ReconstructPath(source, target, meetingHub, distance);
    }

    private void Preprocess()
    {
        // Order nodes by importance (simplified - use degree-based ordering)
        _nodesOrdered = _graph.GetNodes()
            .OrderByDescending(n => _graph.GetDegree(n))
            .ToList();

        // Initialize labels
        foreach (var node in _nodesOrdered)
        {
            _forwardLabels[node] = new List<(T, TWeight)>();
            _backwardLabels[node] = new List<(T, TWeight)>();
        }

        // Compute labels in order
        foreach (var node in _nodesOrdered)
        {
            ComputeForwardLabels(node);
            ComputeBackwardLabels(node);
        }

        // Prune redundant labels
        PruneLabels();
    }

    private void ComputeForwardLabels(T node)
    {
        var distances = new Dictionary<T, TWeight>();
        var queue = new List<T> { node };
        distances[node] = ArithmeticOperations<TWeight>.Zero;

        while (queue.Any())
        {
            var current = queue.OrderBy(n => distances[n]).First();
            queue.Remove(current);

            // Add current as hub if it comes after node in ordering
            if (_nodesOrdered.IndexOf(current) >= _nodesOrdered.IndexOf(node))
            {
                _forwardLabels[node].Add((current, distances[current]));
            }

            // Continue search
            foreach (var neighbor in _graph.GetNeighbors(current))
            {
                if (!_graph.ContainsEdge(current, neighbor)) continue;

                var edgeWeight = _graph.GetEdgeWeight(current, neighbor);
                var newDistance = _add(distances[current], edgeWeight);

                if (!distances.ContainsKey(neighbor) || _compare(newDistance, distances[neighbor]) < 0)
                {
                    distances[neighbor] = newDistance;
                    queue.Add(neighbor);
                }
            }
        }

        // Sort forward labels by hub order
        _forwardLabels[node] = _forwardLabels[node]
            .OrderBy(label => _nodesOrdered.IndexOf(label.Hub))
            .ToList();
    }

    private void ComputeBackwardLabels(T node)
    {
        var distances = new Dictionary<T, TWeight>();
        var queue = new List<T> { node };
        distances[node] = ArithmeticOperations<TWeight>.Zero;

        while (queue.Any())
        {
            var current = queue.OrderBy(n => distances[n]).First();
            queue.Remove(current);

            // Add current as hub if it comes after node in ordering
            if (_nodesOrdered.IndexOf(current) >= _nodesOrdered.IndexOf(node))
            {
                _backwardLabels[node].Add((current, distances[current]));
            }

            // Continue search (reverse edges)
            foreach (var neighbor in _graph.GetNeighbors(current))
            {
                if (!_graph.ContainsEdge(neighbor, current)) continue;

                var edgeWeight = _graph.GetEdgeWeight(neighbor, current);
                var newDistance = _add(distances[current], edgeWeight);

                if (!distances.ContainsKey(neighbor) || _compare(newDistance, distances[neighbor]) < 0)
                {
                    distances[neighbor] = newDistance;
                    queue.Add(neighbor);
                }
            }
        }

        // Sort backward labels by hub order
        _backwardLabels[node] = _backwardLabels[node]
            .OrderBy(label => _nodesOrdered.IndexOf(label.Hub))
            .ToList();
    }

    private void PruneLabels()
    {
        // Prune forward labels
        foreach (var node in _nodesOrdered)
        {
            var prunedLabels = new List<(T, TWeight)>();
            
            foreach (var label in _forwardLabels[node])
            {
                bool isDominated = false;
                
                // Check if this label is dominated by any other label
                foreach (var otherLabel in _forwardLabels[node])
                {
                    if (!label.Hub.Equals(otherLabel.Hub) && 
                        _compare(otherLabel.Distance, label.Distance) <= 0)
                    {
                        // Check if otherLabel reaches all hubs that label reaches
                        bool dominates = true;
                        foreach (var (hub, distance) in _forwardLabels[label.Hub])
                        {
                            var otherDistance = GetHubDistance(otherLabel.Hub, hub);
                            if (_compare(_add(otherLabel.Distance, otherDistance), distance) > 0)
                            {
                                dominates = false;
                                break;
                            }
                        }
                        
                        if (dominates)
                        {
                            isDominated = true;
                            break;
                        }
                    }
                }
                
                if (!isDominated)
                {
                    prunedLabels.Add(label);
                }
            }
            
            _forwardLabels[node] = prunedLabels;
        }

        // Prune backward labels similarly
        foreach (var node in _nodesOrdered)
        {
            var prunedLabels = new List<(T, TWeight)>();
            
            foreach (var label in _backwardLabels[node])
            {
                bool isDominated = false;
                
                foreach (var otherLabel in _backwardLabels[node])
                {
                    if (!label.Hub.Equals(otherLabel.Hub) && 
                        _compare(otherLabel.Distance, label.Distance) <= 0)
                    {
                        bool dominates = true;
                        foreach (var (hub, distance) in _backwardLabels[label.Hub])
                        {
                            var otherDistance = GetHubDistance(otherLabel.Hub, hub);
                            if (_compare(_add(otherLabel.Distance, otherDistance), distance) > 0)
                            {
                                dominates = false;
                                break;
                            }
                        }
                        
                        if (dominates)
                        {
                            isDominated = true;
                            break;
                        }
                    }
                }
                
                if (!isDominated)
                {
                    prunedLabels.Add(label);
                }
            }
            
            _backwardLabels[node] = prunedLabels;
        }
    }

    private (TWeight Distance, T MeetingHub) QueryDistance(T source, T target)
    {
        var minDistance = _infinity;
        T meetingHub = default!;

        // Find common hubs between forward labels of source and backward labels of target
        var sourceLabels = _forwardLabels[source];
        var targetLabels = _backwardLabels[target];

        // Use two-pointer technique since labels are sorted by hub order
        int i = 0, j = 0;
        
        while (i < sourceLabels.Count && j < targetLabels.Count)
        {
            var sourceLabel = sourceLabels[i];
            var targetLabel = targetLabels[j];
            
            var sourceOrder = _nodesOrdered.IndexOf(sourceLabel.Hub);
            var targetOrder = _nodesOrdered.IndexOf(targetLabel.Hub);
            
            if (sourceLabel.Hub.Equals(targetLabel.Hub))
            {
                var totalDistance = _add(sourceLabel.Distance, targetLabel.Distance);
                if (_compare(totalDistance, minDistance) < 0)
                {
                    minDistance = totalDistance;
                    meetingHub = sourceLabel.Hub;
                }
                i++;
                j++;
            }
            else if (sourceOrder < targetOrder)
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return (minDistance, meetingHub);
    }

    private TWeight GetHubDistance(T from, T to)
    {
        // Get distance from hub labels
        if (_forwardLabels.ContainsKey(from))
        {
            var label = _forwardLabels[from].FirstOrDefault(l => l.Hub.Equals(to));
            if (label.Hub != null)
            {
                return label.Distance;
            }
        }
        
        return _infinity;
    }

    private PathResult<T, TWeight> ReconstructPath(T source, T target, T meetingHub, TWeight distance)
    {
        // Reconstruct path: source -> meetingHub -> target
        var sourcePath = ReconstructPathToHub(source, meetingHub, true);
        var targetPath = ReconstructPathToHub(target, meetingHub, false);
        
        targetPath.Reverse();
        
        // Remove duplicate meeting hub
        if (sourcePath.Count > 1 && targetPath.Count > 1)
        {
            sourcePath.RemoveAt(sourcePath.Count - 1);
        }
        
        sourcePath.AddRange(targetPath);

        return new PathResult<T, TWeight>(source, target, distance, sourcePath, "Hub Labeling", 0);
    }

    private List<T> ReconstructPathToHub(T start, T hub, bool forward)
    {
        var path = new List<T>();
        var current = start;
        path.Add(current);

        while (!current.Equals(hub))
        {
            T next = default!;
            TWeight minDistance = _infinity;
            
            var labels = forward ? _forwardLabels[current] : _backwardLabels[current];
            
            foreach (var (labelHub, distance) in labels)
            {
                if (_nodesOrdered.IndexOf(labelHub) >= _nodesOrdered.IndexOf(current))
                {
                    var remainingDistance = GetHubDistance(labelHub, hub);
                    var totalDistance = _add(distance, remainingDistance);
                    
                    if (_compare(totalDistance, minDistance) < 0)
                    {
                        minDistance = totalDistance;
                        next = labelHub;
                    }
                }
            }
            
            if (next == null || next.Equals(current))
            {
                // Fallback to Dijkstra for reconstruction
                var dijkstraResult = Dijkstra<T, TWeight>.FindShortestPath(_graph, current, hub);
                if (dijkstraResult.Path.Any())
                {
                    path.RemoveAt(path.Count - 1); // Remove current
                    path.AddRange(dijkstraResult.Path);
                    return path;
                }
                break;
            }
            
            current = next;
            path.Add(current);
        }

        return path;
    }

    /// <summary>
    /// Gets the forward labels for a node.
    /// </summary>
    public IReadOnlyCollection<(T Hub, TWeight Distance)> GetForwardLabels(T node)
    {
        return _forwardLabels.TryGetValue(node, out var labels) ? 
            new ReadOnlyCollection<(T, TWeight)>(labels) : 
            new ReadOnlyCollection<(T, TWeight)>(new List<(T, TWeight)>());
    }

    /// <summary>
    /// Gets the backward labels for a node.
    /// </summary>
    public IReadOnlyCollection<(T Hub, TWeight Distance)> GetBackwardLabels(T node)
    {
        return _backwardLabels.TryGetValue(node, out var labels) ? 
            new ReadOnlyCollection<(T, TWeight)>(labels) : 
            new ReadOnlyCollection<(T, TWeight)>(new List<(T, TWeight)>());
    }
}
