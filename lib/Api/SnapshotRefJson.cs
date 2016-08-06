using System.Linq;
using Newtonsoft.Json;

namespace lib
{
	public class SnapshotRefJson
	{
		[JsonProperty("snapshot_time")]
		public int Time;
		[JsonProperty("snapshot_hash")]
		public string Hash;
	}
	public class SnapshotJson
	{
		[JsonProperty("problems")]
		public ProblemJson[] Problems;
	}
	public class ProblemJson
	{
		[JsonProperty("problem_spec_hash")]
		public string SpecHash;
		[JsonProperty("problem_size")]
		public int ProblemSize;
		[JsonProperty("ranking")]
		public RankingJson[] Ranking;

		public double ExpectedScore()
		{
			var n = Ranking.Count(r => r.resemblance > 0.999999);
			return ProblemSize/(n + 1.0);
		}

		[JsonProperty("problem_id")] public int Id;
	}

	public class RankingJson
	{
		[JsonProperty("resemblance")]
		public double resemblance;
	}
}