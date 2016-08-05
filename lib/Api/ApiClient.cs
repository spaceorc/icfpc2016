using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
			return Query("hello");
		}
		public JObject Snapshots()
		{
			return Query("snapshot/list");
		}

		public JObject GetBlob(string hash)
		{
			return Query($"blob/{hash}");
		}

		private JObject Query(string query)
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
				var ans = client.GetStringAsync($"{baseUrl}{query}").Result;
				return JObject.Parse(ans);
			}
		}
	}

	[TestFixture]
	public class ApiClient_Should
	{
		[Test]
		public void AccessHelloWorld()
		{
			Console.WriteLine(new ApiClient().HelloWorld());
		}
	}
}