using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
			return ((JArray)res["snapshots"]).Select(t => t.ToObject<SnapshotRefJson>())
				.OrderBy(s => s.Time)
				.ToArray();
		}

		public SnapshotJson GetLastSnapshot()
		{
			var hash = GetSnapshots().Last().Hash;
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
		private string Query(string query)
		{
			var handler = new HttpClientHandler()
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 3
			};
			using (var client = new HttpClient(handler))
			{
				client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
				return client.GetStringAsync($"{baseUrl}{query}").Result;
			}
		}
	}

	public static class Paths
	{
		public static string ProblemsDir() => 
			Path.Combine(new DirectoryInfo(TestContext.CurrentContext.TestDirectory).Parent?.Parent?.Parent?.FullName ?? ".", "problems");

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
			foreach (var p in snapshot.Problems.Skip(10))
			{
				var spec = api.GetBlob(p.SpecHash);
				var filepath = Path.Combine(Paths.ProblemsDir(), $@"{p.Id:000}.spec.txt");
				Console.WriteLine($"writing {filepath}");
				File.WriteAllText(filepath, spec);
			}
		}
	}
}