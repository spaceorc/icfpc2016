using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
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
			return ProblemSpec.Parse(File.ReadAllText(GetFilename(id)));
		}

		private string GetFilename(int id)
		{
			return Path.Combine(problemsDir, $"{id:000}.spec.txt");
		}

		public IEnumerable<ProblemSpec> GetAll()
		{
			return
				Enumerable.Range(1, int.MaxValue)
					.Select(id => new { id, fn = GetFilename(id) })
					.TakeWhile(p => File.Exists(p.fn))
					.Select(p => ProblemSpec.Parse(File.ReadAllText(p.fn), p.id));

		}
		public void PutResponse(int id, string response)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.response.txt"), response);
		}

		public void PutSolution(int id, SolutionSpec solutionSpec)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.solution.txt"), solutionSpec.ToString());
		}

		public void PutSolution(int id, string solutionSpec)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.solution.txt"), solutionSpec);
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