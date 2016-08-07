using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using FluentAssertions;
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