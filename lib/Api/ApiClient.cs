using System;
using System.Collections.Generic;
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
			using (var client = CreateClient())
			{
				var content = new MultipartFormDataContent();
				content.Add(new StringContent(problemId.ToString()), "problem_id");
				content.Add(new StringContent(solution.ToString()), "solution_spec", "solution.txt");
				//workaround: http://stackoverflow.com/questions/31129873/make-http-client-synchronous-wait-for-response
				var res = client.PostAsync($"{baseUrl}solution/submit", content).ConfigureAwait(false).GetAwaiter().GetResult();
				if (!res.IsSuccessStatusCode)
				{
					Console.WriteLine(res.ToString());
					Console.WriteLine(res.Content.ReadAsStringAsync().Result);
					throw new HttpRequestException(res.ReasonPhrase);
				}
				return res.Content.ReadAsStringAsync().Result;
			}
		}

		public string PostProblem(long publishTime, SolutionSpec solution)
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


		private string Query(string query)
		{
			using (var client = CreateClient())
				return client.GetStringAsync($"{baseUrl}{query}").Result;
		}

		private HttpClient CreateClient()
		{
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
		}
	}
}