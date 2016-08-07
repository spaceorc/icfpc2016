using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalUtilities.Utilities;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace lib.ProjectionSolver
{
    public class PerimeterFinder
    {
	    private const bool ShowDebug = true;

	    public PerimeterFinder(WayFinder wayFinder, Rational[] pathLengths)
	    {
		    this.wayFinder = wayFinder;
		    this.pathLengths = pathLengths;

			alreadyYeildedPaths = new HashSet<string>();
	    }

	    private void DebugPathMetrics(params int[] nodeNumbers)
	    {
		    foreach (var dictByStart in wayFinder.Result.Values)
		    {
			    var paths = dictByStart[nodeNumbers[0]];
			    foreach (var path in paths)
			    {
				    var curNodeNumbers = path.edges.Select(edge => edge.From.NodeNumber).ToList();
					curNodeNumbers.Add(path.LastEdge.To.NodeNumber);
				    if (curNodeNumbers.SequenceEqual(nodeNumbers))
					{
						Console.WriteLine($"path = {path}");
						Console.WriteLine($"  .originalityByVertices = {path.originalityByVertices}");
						Console.WriteLine($"  .originalityByEdges = {path.originalityByEdges}");
						Console.WriteLine($"  .straightness = {path.straightness}");
						return;
				    }
			    }
		    }
	    }

	    public IEnumerable<List<PPath>> Find(double maxTotalPenalty = 0.4)
	    {
			for (var i = 0; i < 10; i++)
				wayFinder.MakeIteration();

		    var iter = 0;
		    var needMoreIterations = true;
			while (needMoreIterations)
			{
				if (ShowDebug)
					Console.WriteLine("\n === Let's make one more iteration ===");
				wayFinder.MakeIteration();
				iter++;

				needMoreIterations = false;
				for (var start = 0; start < wayFinder.Graph.NodesCount; start++)
				{
					if (ShowDebug)
						Console.WriteLine($"iter = {iter}, start = {start}");

					foreach (var perimeter in FindRecursively(start, start, maxTotalPenalty, new Stack<PPath>()))
					{
						if (perimeter == null)
						{
							needMoreIterations = true;
							continue;
						}
						yield return perimeter;
					}
				}
			}
	    }

	    private IEnumerable<List<PPath>> FindRecursively(int loopStartNode, int nodeToContinueFrom, double availablePenalty, Stack<PPath> perimeterStack)
	    {
		    var lengthIndex = perimeterStack.Count;
			if (lengthIndex == pathLengths.Length)
		    {
			    if (nodeToContinueFrom == loopStartNode)
			    {
				    var perimeter = perimeterStack.Select(ppath => new PPath(ppath)).Reverse().ToList();
				    if (HasNotBeenYieldedEarlier(perimeter))
				    {
					    yield return perimeter;
					}
				}
			    yield break;
		    }

		    var length = pathLengths[lengthIndex];
		    var pathCandidates = wayFinder.Result.GetValueOrDefault(length)?.GetValueOrDefault(nodeToContinueFrom) ?? new List<PPath>();

			foreach (var pathCandidate in pathCandidates)
			{
				var penaltyForPath = 1 - pathCandidate.metric;
				var remainingPenalty = availablePenalty - penaltyForPath;
				if (remainingPenalty < 0)
					yield break;
				
				perimeterStack.Push(pathCandidate);
			    foreach (var perimeter in FindRecursively(loopStartNode, pathCandidate.LastEdge.To.NodeNumber, remainingPenalty, perimeterStack))
				    yield return perimeter;
			    perimeterStack.Pop();
		    }
			if (perimeterStack.Count == pathLengths.Length - 1)
				yield return null; // Значит, availablePenalty не было израсходовано и после wayFinder.MakeIteration() можно поискать ещё путей
		}

	    private bool HasNotBeenYieldedEarlier(List<PPath> perimeter)
	    {
		    if (perimeter.Count != 4 || pathLengths[0] != pathLengths[2] || pathLengths[1] != pathLengths[3])
				throw new ArgumentException("PerimeterFinder.CheckRepetitions can't work with this parameters");

			var a = string.Join(",", perimeter[0].NodeNumbers) + ".";
			var b = string.Join(",", perimeter[1].NodeNumbers) + ".";
			var c = string.Join(",", perimeter[2].NodeNumbers) + ".";
			var d = string.Join(",", perimeter[3].NodeNumbers) + ".";
		    return 
				alreadyYeildedPaths.Add(a + b + c + d) &&
				alreadyYeildedPaths.Add(c + d + a + b) &&
				alreadyYeildedPaths.Add(d + c + b + a) &&
				alreadyYeildedPaths.Add(b + a + d + c);
		}

		private bool availablePenaltyExhausted;

		private readonly HashSet<string> alreadyYeildedPaths;

	    private readonly WayFinder wayFinder;
	    private readonly Rational[] pathLengths;
    }
}
