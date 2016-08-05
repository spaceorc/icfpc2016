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

		[JsonProperty("problem_id")] public int Id;
	}


}