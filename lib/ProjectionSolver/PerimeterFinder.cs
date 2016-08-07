using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalUtilities.Utilities;

namespace lib.ProjectionSolver
{
    public class PerimeterFinder
    {
	    public PerimeterFinder(WayFinder wayFinder, Rational[] pathLengths)
	    {
		    this.wayFinder = wayFinder;
		    this.pathLengths = pathLengths;

			alreadyYieldedPaths = new HashSet<string>();
	    }

	    private void DebugPathMetrics(params int[] nodeNumbers)
	    {
		    foreach (var dictByStart in wayFinder.Result.Values)
		    {
                if (!dictByStart.ContainsKey(nodeNumbers[0])) continue;
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
                        Console.WriteLine($"  .index={paths.IndexOf(path)}/{paths.Count}");
						return;
				    }
			    }
		    }
	    }

	    public IEnumerable<List<PPath>> Find(double cutOffBorder)
	    {
			for (var i = 0; i < 8; i++)
				wayFinder.MakeIteration();

            DebugPathMetrics(5, 10, 3);
            DebugPathMetrics(2, 0, 13, 14, 8, 7, 12, 11, 5);
            DebugPathMetrics(6, 11, 13, 1, 9, 14, 12, 4, 3);
            DebugPathMetrics(6,10,2);


            var iter = 0;
			while (true)
			{
				Console.WriteLine("\n === Let's make one more iteration ===");
//				wayFinder.MakeIteration();
				iter++;

				for (var start = 0; start < wayFinder.Graph.NodesCount; start++)
					for (var indexSum = 0; ; indexSum++)
					{
						Console.WriteLine($"iter = {iter}, start = {start}, indexSum = {indexSum}");

						indexSumTooBig = true;

						foreach (var perimeter in FindRecursively(start, start, indexSum, new Stack<PPath>(), new Stack<int>(), cutOffBorder))
							yield return perimeter;

						if (indexSumTooBig)
							break;
					}
			}
	    }

	    private IEnumerable<List<PPath>> FindRecursively(int loopStartNode, int nodeToContinueFrom, int indexSum, Stack<PPath> perimeterStack, Stack<int> debug, double cutOffBorder)
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
						// Console.WriteLine("debug: {0}", string.Join(" ", debug));
					}
				}
			    yield break;
		    }

		    var length = pathLengths[lengthIndex];
		    var pathCandidates = wayFinder.Result.GetValueOrDefault(length)?.GetValueOrDefault(nodeToContinueFrom) ?? new List<PPath>();

		    var i = 0;
		    if (lengthIndex == pathLengths.Length - 1)
		    {
			    i = indexSum; // Сразу выбираем последний индекс так, чтобы сумма всех индексов в точности была indexSum

			    if (i < pathCandidates.Count)
				    indexSumTooBig = false;
		    }

		    for (; i <= indexSum && i < pathCandidates.Count; i++)
		    {
			    var pathCandidate = pathCandidates[i];
				if (pathCandidate.metric < cutOffBorder)
					continue;
				
				perimeterStack.Push(pathCandidate);
				debug.Push(i);
			    foreach (var perimeter in FindRecursively(loopStartNode, pathCandidate.LastEdge.To.NodeNumber, indexSum - i, perimeterStack, debug, cutOffBorder))
				    yield return perimeter;
				debug.Pop();
			    perimeterStack.Pop();
		    }
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
				alreadyYieldedPaths.Add(a + b + c + d) &&
				alreadyYieldedPaths.Add(c + d + a + b) &&
				alreadyYieldedPaths.Add(d + c + b + a) &&
				alreadyYieldedPaths.Add(b + a + d + c);
	    }

	    private readonly HashSet<string> alreadyYieldedPaths;

	    private bool indexSumTooBig;

	    private readonly WayFinder wayFinder;
	    private readonly Rational[] pathLengths;
    }
}
