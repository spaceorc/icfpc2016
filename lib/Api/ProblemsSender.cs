using lib;
using Newtonsoft.Json.Linq;
using Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lib.Api
{
	public class ProblemsSender
    {

        public static double SolveAndSend(int id)
        {
            var repo = new ProblemsRepo();
            var problemSpec = repo.Get(id);
            var spec = ProjectionSolverRunner.Solve(problemSpec);
            return Post(problemSpec, spec);
        }

        public static double Post(ProblemSpec problemSpec, SolutionSpec solutionSpec)
        {
            var client = new ApiClient();
            var repo = new ProblemsRepo();
            try
            {
                var response = client.PostSolution(problemSpec.id, solutionSpec);
                repo.PutResponse(problemSpec.id, response);
                repo.PutSolution(problemSpec.id, solutionSpec);
                var obj = JObject.Parse(response);
                return obj["resemblance"].Value<double>();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                return 0;
            }
            finally
            {
                Thread.Sleep(1000);
            }
        }
    }
}
