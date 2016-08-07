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

	    public IEnumerable<List<PPath>> Find(double cutOffBorder)
	    {
			for (var i = 0; i < 20; i++)
				wayFinder.MakeIteration();

		    var iter = 0;
			while (true)
			{
				Console.WriteLine("\n === Let's make one more iteration ===");
				wayFinder.MakeIteration();
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

	    private IEnumerable<List<PPath>> FindRecursively(int loopStartNode, int nodeToContinueFrom, int indexSum, Stack<PPath> currentPerimeter, Stack<int> debug, double cutOffBorder)
	    {
		    var lengthIndex = currentPerimeter.Count;
			if (lengthIndex == pathLengths.Length)
		    {
			    if (nodeToContinueFrom == loopStartNode)
			    {
				    yield return currentPerimeter.Select(ppath => new PPath(ppath)).Reverse().ToList();
					// Console.WriteLine("debug: {0}", string.Join(" ", debug));
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
				
				currentPerimeter.Push(pathCandidate);
				debug.Push(i);
			    foreach (var perimeter in FindRecursively(loopStartNode, pathCandidate.LastEdge.To.NodeNumber, indexSum - i, currentPerimeter, debug, cutOffBorder))
				    yield return perimeter;
				debug.Pop();
			    currentPerimeter.Pop();
		    }
	    }

	    private bool indexSumTooBig;

	    private readonly WayFinder wayFinder;
	    private readonly Rational[] pathLengths;
    }
}
