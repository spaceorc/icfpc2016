using System.IO;

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
			return ProblemSpec.Parse(File.ReadAllText(Path.Combine(problemsDir, $"{id:000}.spec.txt")));
		}

		public void PutResponse(int id, string response)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.response.txt"), response);
		}

		public void PutSolution(int id, SolutionSpec solutionSpec)
		{
			File.WriteAllText(Path.Combine(problemsDir, $"{id:000}.solution.txt"), solutionSpec.ToString());
		}
	}
}