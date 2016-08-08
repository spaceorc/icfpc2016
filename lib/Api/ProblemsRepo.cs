using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace lib
{
	public class ProblemsRepo
	{
		private readonly string problemsDir;

		public ProblemsRepo()
			: this(Paths.ProblemsDir())
		{
		}

		public ProblemsRepo(string problemsDir)
		{
			this.problemsDir = problemsDir;
		}

		public ProblemSpec Get(int id)
		{
            var s= ProblemSpec.Parse(File.ReadAllText(GetFilename(id)));
            s.id = id;
            return s;
		}

		public ProblemSpec Find(int id)
		{
			return File.Exists(GetFilename(id)) ? ProblemSpec.Parse(File.ReadAllText(GetFilename(id))) : null;
		}

		private string GetFilename(int id)
		{
			return Path.Combine(problemsDir, $"{id:000}.spec.txt");
		}

		public IEnumerable<ProblemSpec> GetAll()
		{
			return Directory.GetFiles(problemsDir, "*.spec.txt")
				.Select(p => ProblemSpec.Parse(File.ReadAllText(p), ExtractProblemId(p))).ToList();
		}

		public IEnumerable<ProblemSpec> GetAllNotSolvedPerfectly()
		{
			return GetAll().Where(x => GetProblemResemblance(x.id) < 1.0).ToList();
		}

		public IEnumerable<Tuple<string, int>> GetAllProblemSpecContentAndId()
		{
			return Directory.GetFiles(problemsDir, "*.spec.txt")
				.Select(p => Tuple.Create(File.ReadAllText(p), ExtractProblemId(p))).ToList();
		}

		private static int ExtractProblemId(string fileName)
		{
			return int.Parse(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName) ?? ""));
		}

		public void PutResponse(int id, string response)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.response.txt"), response);
		}

		public string FindResponse(int id)
		{
			var path = Path.Combine(problemsDir, $"{id:000}.response.txt");
			return !File.Exists(path) ? null : File.ReadAllText(path);
		}

		public double GetProblemResemblance(int problemId)
		{
			var response = FindResponse(problemId);
			return response == null ? 0.0 : GetResemblance(response);
		}

		public double GetResemblance(string response)
		{
			return JObject.Parse(response)["resemblance"].Value<double>();
		}

		public void PutSolution(int id, SolutionSpec solutionSpec)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.solution.txt"), solutionSpec.ToString());
		}

		public void PutSolution(int id, string solutionSpec)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.solution.txt"), solutionSpec);
		}

		public void Put(int id, string problemSpec)
		{
			File.WriteAllText(GetFilename(id), problemSpec);
		}

		public SnapshotJson GetSnapshot(ApiClient api)
		{
			var path = Path.Combine(problemsDir, $"snapshot.txt");
			if (api != null && (!File.Exists(path) || File.GetLastWriteTime(path) < DateTime.Now - TimeSpan.FromHours(1)))
			{
				try
				{
					var res = api.GetLastSnapshotString();
					File.WriteAllText(path, res);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Cant donwload fresh snapshot");
				}
			}
			return JsonConvert.DeserializeObject<SnapshotJson>(File.ReadAllText(path));
		}

		public string FindSolution(int id)
		{
			var path = Path.Combine(problemsDir, $"{id:000}.solution.txt");
			return !File.Exists(path) ? null : File.ReadAllText(path);
		}
	}

	[TestFixture]
	public class ProblemsRepo_Should
	{
		[Test]
		public void GetProblem()
		{
			new ProblemsRepo().Get(2).Should().NotBeNull();
		}

		[Test]
		public void GetAll()
		{
			new ProblemsRepo().GetAll().Should().NotBeEmpty();
		}
	}
}