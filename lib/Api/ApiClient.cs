using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using FluentAssertions;
using lib.ProjectionSolver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace lib
{
	public class ApiClient
	{
		private string apiKey = "89-3fd1c2c060315bb0b0d897c922ac4de2";
		private string baseUrl = "http://2016sv.icfpcontest.org/api/";

		public JObject HelloWorld()
		{
			return QueryJson("hello");
		}

		public IEnumerable<SnapshotRefJson> GetSnapshots()
		{
			var res = QueryJson("snapshot/list");
			return ((JArray) res["snapshots"]).Select(t => t.ToObject<SnapshotRefJson>())
				.OrderBy(s => s.Time)
				.ToArray();
		}

		public SnapshotJson GetLastSnapshot()
		{
			var hash = GetSnapshots().Last().Hash;
			Thread.Sleep(1000);
			return GetBlob<SnapshotJson>(hash);
		}
		public string GetLastSnapshotString()
		{
			var hash = GetSnapshots().Last().Hash;
			Thread.Sleep(1000);
			return GetBlob(hash);
		}

		public string GetBlob(string hash)
		{
			return Query($"blob/{hash}");
		}

		public T GetBlob<T>(string hash)
		{
			return JsonConvert.DeserializeObject<T>(GetBlob(hash));
		}

		private JObject QueryJson(string query)
		{
			return JObject.Parse(Query(query));
		}

		public string PostSolution(int problemId, SolutionSpec solution)
		{
			return PostSolution(problemId, solution.ToString());
		}

		private static readonly Stopwatch sw = Stopwatch.StartNew();

		public string PostSolution(int problemId, string solution)
		{
			if (sw.Elapsed < TimeSpan.FromSeconds(1))
				Thread.Sleep(TimeSpan.FromSeconds(1));
			try
			{
				return PostSolutionWithAttempts(problemId, solution);
			}
			finally
			{
				sw.Restart();
			}
		}

		private string PostSolutionWithAttempts(int problemId, string solution)
		{
			var attempt = 0;
			while (true)
			{
				var isRetriableError = true;
				try
				{
					using (var client = CreateClient())
					{
						var content = new MultipartFormDataContent();
						content.Add(new StringContent(problemId.ToString()), "problem_id");
						content.Add(new StringContent(solution), "solution_spec", "solution.txt");
						//workaround: http://stackoverflow.com/questions/31129873/make-http-client-synchronous-wait-for-response
						var res = client.PostAsync($"{baseUrl}solution/submit", content).ConfigureAwait(false).GetAwaiter().GetResult();
						var result = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
						if (!res.IsSuccessStatusCode)
						{
							if (result.Contains("\"ok\":false"))
							{
								Console.ForegroundColor = ConsoleColor.Red;
								Console.Write(result);
								Console.ResetColor();
								isRetriableError = false;
							}
							else
							{
								Console.WriteLine(res.ToString());
								Console.WriteLine(result);
							}
							throw new HttpRequestException(res.ReasonPhrase);
						}
						return result;
					}
				}
				catch (Exception e)
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
					if (!isRetriableError)
						throw;
					Console.WriteLine("Will retry failed solution post for problem: {0}\r\n{1}", problemId, e);
					if (++attempt > 10)
						throw new InvalidOperationException($"Failed to post solution for {problemId} with attempts", e);
				}
			}
		}

		public string PostProblem(long publishTime, SolutionSpec solution)
		{
			if (sw.Elapsed < TimeSpan.FromSeconds(1))
				Thread.Sleep(TimeSpan.FromSeconds(1));
			try
			{
				using (var client = CreateClient())
				{
					var content = new MultipartFormDataContent();
					content.Add(new StringContent(publishTime.ToString()), "publish_time");
					content.Add(new StringContent(solution.ToString()), "solution_spec", "solution.txt");
					//workaround: http://stackoverflow.com/questions/31129873/make-http-client-synchronous-wait-for-response
					var res = client.PostAsync($"{baseUrl}problem/submit", content).ConfigureAwait(false).GetAwaiter().GetResult();
					if (!res.IsSuccessStatusCode)
					{
						Console.WriteLine(res.ToString());
						Console.WriteLine(res.Content.ReadAsStringAsync().Result);
						throw new HttpRequestException(res.ReasonPhrase);
					}
					return res.Content.ReadAsStringAsync().Result;
				}
			}
			finally
			{
				sw.Restart();
			}
		}

		private string Query(string query)
		{
			if (sw.Elapsed < TimeSpan.FromSeconds(1))
				Thread.Sleep(TimeSpan.FromSeconds(1));
			try
			{
				return DoQueryWithAttempts(query);
			}
			finally
			{
				sw.Restart();
			}
		}

		private string DoQueryWithAttempts(string query)
		{
			var attempt = 0;
			while (true)
			{
				try
				{
					using (var client = CreateClient())
						return client.GetStringAsync($"{baseUrl}{query}").Result;
				}
				catch (Exception e)
				{
					Console.WriteLine("Will retry failed query: {0}\r\n{1}", query, e);
					if (++attempt > 10)
						throw new InvalidOperationException($"Query failed with attempts: {query}", e);
					Thread.Sleep(TimeSpan.FromSeconds(1));
				}
			}
		}

		private HttpClient CreateClient()
		{
			AskTimeSlot();
			var handler = new HttpClientHandler()
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 3
			};
			var client = new HttpClient(handler);
			client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
			client.DefaultRequestHeaders.ExpectContinue = false;
			return client;
		}

		private void AskTimeSlot()
		{
			try
			{
				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri("http://spaceorc-t430:666/");
					var message = client.GetAsync("/ask").GetAwaiter().GetResult();
					if (!message.IsSuccessStatusCode)
					{
						Console.WriteLine("Bad response from TimeManager. Just waiting 1 seconds...");
						Thread.Sleep(1000);
						return;
					}
					message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				}
			}
			catch (Exception)
			{
				Console.WriteLine("TimeManager is unavailable");
			}
		}
	}

	[TestFixture, Explicit]
	public class ApiClient_Utils
	{
		[Test]
		public void HelloWorld_SmokeTest()
		{
			var response = new ApiClient().HelloWorld();
			response["ok"].Value<bool>().Should().BeTrue();
		}

		[Test]
		public void GetProblems()
		{
			var api = new ApiClient();
			var snapshot = api.GetLastSnapshot();
			foreach (var p in snapshot.Problems)
			{
				var spec = api.GetBlob(p.SpecHash);
				var filepath = Path.Combine(Paths.ProblemsDir(), $@"{p.Id:000}.spec.txt");
				Console.WriteLine($"writing {filepath}");
				File.WriteAllText(filepath, spec);
			}
			//1763
		}

		[Test]
		public void CalcImperfectScore()
		{
			var snapshotJson = new ProblemsRepo().GetSnapshot(new ApiClient());
			var v = snapshotJson.Problems.Where(p => p.Ranking.All(r => r.resemblance != 1.0))
				.Sum(p => p.SolutionSize / (1 + p.Ranking.Length));
			Console.WriteLine(v);
		}

		[Test, Explicit]
		public void FindRects()
		{
			var repo = new ProblemsRepo();
			var sq = repo.GetAllNotSolvedPerfectly().Where(p => p.Segments.Length == 4);
			foreach (var p in sq)
			{
				Console.WriteLine(p.id);
			}
		}
		[Test, Explicit]
		public void SolveRibbons()
		{
			var repo = new ProblemsRepo();
			var snapshotJson = repo.GetSnapshot(new ApiClient());
			var ribbons = repo.GetAllNotSolvedPerfectly().Where(IsRibbon);
			var sum = 0d;
			foreach (var ribbon in ribbons)
			{
				var desc = snapshotJson.Problems.First(p => p.Id == ribbon.id);
				sum += desc.ExpectedScore();
				Console.WriteLine(ribbon.id + " Owner = " + desc.Owner + " Exp = " + desc.ExpectedScore());
			}
			Console.WriteLine(sum);
		}

		[Test]
		public void CalculateOurScore()
		{
			var repo = new ProblemsRepo();
			var sn = repo.GetSnapshot(null);
			var ourScore1 = 0.0;
			var ourScore2 = 0.0;
			var ourScore3 = 0.0;
			foreach (var pr in sn.Problems)
			{
				var solutions = pr.Ranking.Count(r => r.resemblance == 1.0);
				var part = pr.SolutionSize / (solutions + 1.0);
				var myR = repo.GetProblemResemblance(pr.Id);
				if (myR == 1.0)
				{
					ourScore1 += part;
				}
				else
				{
					var rSum = pr.Ranking.Where(r => r.resemblance != 1.0).Sum(r => r.resemblance);
					ourScore2 += part*myR/rSum;
				}
				if (pr.Owner == "89")
				{
					ourScore3 += (5000 - pr.SolutionSize)/Math.Max(6, solutions + 1.0);
				}
			}
			Console.WriteLine(ourScore1);
			Console.WriteLine(ourScore2);
			Console.WriteLine(ourScore3);
			Console.WriteLine(ourScore1+ourScore2+ ourScore3);
			var prCount = repo.GetAll().Count();
			var noSolvedCount = repo.GetAllNotSolvedPerfectly().Count();
			Console.WriteLine(prCount -noSolvedCount);
			Console.WriteLine(prCount);
		}

		private bool IsRibbon(ProblemSpec prob)
		{
			var ds = prob.Segments.Select(s => s.Direction).ToList();
			var diagCount = ds.Count(d => d.X == d.Y || d.X == -d.Y);
			return diagCount== 2 && ds.All(d => d.X == 0 || d.Y == 0 || d.X == d.Y || d.X == -d.Y);
		}


		[Test, Explicit]
		public void GetTriangles()
		{
			/*
			 5987
			 5988
			 5990
			 6100

			16/9709 по X и по Y
			1077/9709 Ч ширина и высота треугольника
			точка 1,1 Ч это вершина с пр€мым углом
			 
			 */

			var repo = new ProblemsRepo();
			var snapshotJson = repo.GetSnapshot(new ApiClient());
			var ps = snapshotJson.Problems.Where(p => p.Owner == "149");
			foreach (var problem in ps)
			{
				var pr = repo.Get(problem.Id);
				var points = pr.Points.Distinct().ToList();
				if (points.Count == 7)
				{
					var w = points.Max(p => p.X);
					var h = points.Max(p => p.Y);
					var minW1 = points.Select(p => p.X).Where(ww => ww > 0).Min();
					var minW2 = points.Select(p => w - p.X).Where(ww => ww > 0).Min();
					var minH1 = points.Select(p => p.Y).Where(hh => hh > 0).Min();
					var minH2 = points.Select(p => h - p.Y).Where(hh => hh > 0).Min();
					var minW = minW1 > minW2 ? minW2 : minW1;
					var minH = minH1 > minH2 ? minH2 : minH1;

					Console.WriteLine($"{problem.Id}, \"{w}\", \"{h}\", \"{minW}\", \"{minH}\", {minW1 < minW2}");
				}
			}


		}
		[Test]
		public void CalcImperfectScore2()
		{
			var repo = new ProblemsRepo();
			var c = repo.GetAllNotSolvedPerfectly().Count();
			Console.WriteLine(c);
			Console.WriteLine(repo.GetAll().Count());
		}

		[Test]
		public void GetOurProblems()
		{
			var api = new ApiClient();
			var snapshot = api.GetLastSnapshot();
			foreach(var p in snapshot.Problems.Where(p => p.Owner == "89"))
				Console.WriteLine($"problem {p.Id}, max score {p.Ranking?.Max(rank => rank?.resemblance) ?? 0.0}");
		}

		[Test]
		public void ProblemsRating()
		{
			var api = new ApiClient();
			var repo = new ProblemsRepo();
			var snapshot = api.GetLastSnapshot();
			var totalConvex = 0.0;
			foreach (var p in snapshot.Problems.OrderByDescending(p => p.ExpectedScore()))
			{
				var spec = repo.Find(p.Id);
				var expectedScore = p.ExpectedScore();
				var convex = spec == null ? null : (spec.Polygons.Length == 1 && spec.Polygons[0].IsConvex() && spec.Polygons[0].GetSignedSquare() > 0).ToString();
				Console.WriteLine($"id={p.Id} size={p.SolutionSize} expected={expectedScore} isconvex={convex ?? "Unknown"}");
				if (convex != null) totalConvex += expectedScore;
			}
			Console.WriteLine($"Total convex: {totalConvex}");
		}
	}
}