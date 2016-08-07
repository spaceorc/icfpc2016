using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class SolvedProblemsFinder_Should
	{
		[Test]
		public void DoSomething_WhenSomething()
		{
			SolvedProblemsFinder.Run();
		}
	}

	internal static class SolvedProblemsFinder
	{
		public static void Run()
		{
			var pr = new ProblemsRepo();
			var problemSpecs = pr.GetAllProblemSpecContentAndId().ToList();
			var specSolutionResponse = problemSpecs.ToDictionary(p => p.Item2, p => Tuple.Create(
				p.Item1, //spec
				pr.FindSolution(p.Item2), //sln
				pr.FindResponse(p.Item2), //resp
				p.Item2
				));
			var sameSpecs = new Dictionary<string, List<Tuple<string, string, string, int>>>();
			foreach (var pair in specSolutionResponse)
			{
				if (!sameSpecs.ContainsKey(pair.Value.Item1))
					sameSpecs[pair.Value.Item1] = new List<Tuple<string, string, string, int>>();
				sameSpecs[pair.Value.Item1].Add(pair.Value);
			}
			var toSolve = new Dictionary<int, List<int>>();
			foreach (var taskInfos in sameSpecs)
			{
				var sln = "";
				var id = 0;
				foreach (var taskInfo in taskInfos.Value)
				{
					if (sln != "" || taskInfo.Item3 != null && !taskInfo.Item3.Contains("\"resemblance\":1.0")) continue;
					sln = taskInfo.Item3;
					id = taskInfo.Item4;
				}
				if (sln == "")
					continue;
				var list = taskInfos.Value
					.Where(taskInfo => taskInfo.Item3 == null || !taskInfo.Item3.Contains("\"resemblance\":1.0"))
					.Select(taskInfo => taskInfo.Item4)
					.ToList();
				if (list.Count != 0)
					toSolve[id] = list;
			}

			var client = new ApiClient();
			foreach (var kvp in toSolve)
			{
				if (kvp.Value.Count == 0)
					continue;
				var sln = pr.FindSolution(kvp.Key);
				if (sln == null)
					continue;
				foreach (var id in kvp.Value)
					try
					{
						var response = client.PostSolution(id, sln);
						pr.PutResponse(id, response);
						pr.PutSolution(id, sln);
						Console.WriteLine(JObject.Parse(response)["resemblance"].Value<double>());
					}
					catch (Exception e)
					{
						Console.WriteLine();
						Console.WriteLine(e);
						Console.WriteLine(0);
					}
					finally
					{
						Thread.Sleep(1000);
					}
			}

			//foreach (var problemSpec in specSolutionResponse)
			//{
			//	if (problemSpec.Value.resp == null || !problemSpec.Value.resp.Contains("\"resemblance\":1.0"))
			//		continue;
			//	if (perfectSolutions.Contains(problemSpec.Value.sln))
			//		continue;
			//	perfectSolutions.Add(problemSpec.Value.sln);
			//	toSolve[problemSpec.Key] = specSolutionResponse
			//		.Where(p => p.Key != problemSpec.Key)
			//		.Where(p => p.Value.resp == null || !p.Value.resp.Contains("\"resemblance\":1.0"))
			//		.Where(ps => ps.Value.spec == problemSpec.Value.spec)
			//		.Select(p => p.Key)
			//		.ToList();
			//}
		}
	}
}