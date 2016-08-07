using Newtonsoft.Json.Linq;
using lib.ProjectionSolver;
using System;
using System.Threading;

namespace lib.Api
{
	public class ProblemsSender
	{

		public static double TrySolveAndSend(ProblemSpec problemSpec)
		{
			double res = 0.0;
			var t = new Thread(() =>
			{
				var spec = ProjectionSolverRunner.Solve(problemSpec);
				res = Post(problemSpec, spec);
			})
			{ IsBackground = true };
			t.Start();
			if (!t.Join(5000))
			{
				t.Abort();
				t.Join();
			}
			return res;
		}


        public static double SolveAndSendStrip(int id, Rational otherSide)
        {
            var repo = new ProblemsRepo();
            var problemSpec = repo.Get(id);
            var solver = SolverMaker.Solve(SolverMaker.CreateSolver(problemSpec), otherSide, 0);

            if (solver == null) return 0;

            var cycleFinder = new CycleFinder<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(
               solver.Projection,
               n => n.Data.Projection);

            var cycles = cycleFinder.GetCycles();
            var reflectedCycles = CycleReflector.GetUnribbonedCycles(cycles);
            var spec = ProjectionSolverRunner.GetSolutionsFromCycles(reflectedCycles);

            spec = spec.Pack();
            if (spec == null) return 0;
            return Post(problemSpec, spec);
        }

		public static double SolveAndSend(int id)
		{
			var repo = new ProblemsRepo();
			var problemSpec = repo.Get(id);
			var spec = ProjectionSolverRunner.Solve(problemSpec);
            
            spec=spec.Pack();
            if (spec == null) return 0;
			return Post(problemSpec, spec);
		}
		
        public static double Post(ProblemSpec problemSpec, SolutionSpec solutionSpec)
        {
	        solutionSpec = solutionSpec.Pack();
			var client = new ApiClient();
            var repo = new ProblemsRepo();
            try
            {
				var oldResemblance = repo.GetProblemResemblance(problemSpec.id);
				var response = client.PostSolution(problemSpec.id, solutionSpec);
				var obj = JObject.Parse(response);
				var resemblance = obj["resemblance"].Value<double>();
	            if (resemblance > oldResemblance)
	            {
		            repo.PutResponse(problemSpec.id, response);
		            repo.PutSolution(problemSpec.id, solutionSpec);
		            Console.Out.Write("solution improved! ");
	            }
	            return resemblance;
            }
            catch (Exception e)
            {
	            if (e is ThreadAbortException)
		            return 0;
				Console.WriteLine(e);
				return 0;
			}
		}
	}
}
