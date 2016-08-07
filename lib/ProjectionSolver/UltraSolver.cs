using System;
using lib.Api;
using lib.ProjectionSolver;

namespace lib
{
    public class UltraSolver
    {
        public static SolutionSpec AutoSolve(ProblemSpec problemSpec)
        {
            return Solve(problemSpec, 0.3);
        }

        public static void SolveAndShow(int taskNumber)
        {
            var problemSpec = new ProblemsRepo().Get(taskNumber);
            var solver = Solve2(problemSpec, 0);
            if (solver != null)
                SolverMaker.Visualize(solver);
        }

        public static void SolveAndSend(int taskNumber, bool wait = true, double originality = 0.3)
        {
            var solveAndSendResult = SolveAndSendInternal(taskNumber, originality);
            Console.WriteLine(taskNumber + ":" + " " + solveAndSendResult);
            if (wait)
                Console.ReadKey();
        }

        private static double SolveAndSendInternal(int taskNumber, double originality)
        {
            var problemSpec = new ProblemsRepo().Get(taskNumber);

            var solutionSpec = Solve(problemSpec, originality);
            if (solutionSpec == null)
                return 0;
            return ProblemsSender.Post(taskNumber, solutionSpec);
        }

        private static SolutionSpec Solve(ProblemSpec problemSpec, double originality)
        {
            var solver = SolverMaker.CreateSolver(problemSpec);
            var ribbonWidth = RibbonIndicator.GetRibbonWidth(problemSpec);
            var simpleSolver = SolverMaker.Solve(solver, ribbonWidth.HasValue ? ribbonWidth.Value : 1, originality);
            if (simpleSolver != null)
                return SolutionSpecBuilder.BuildSolutionByRibbonGraph(simpleSolver.Projection);
            return null;
        }

        private static PointProjectionSolver Solve2(ProblemSpec problemSpec, double originality)
        {
            var solver = SolverMaker.CreateSolver(problemSpec);
            var ribbonWidth = RibbonIndicator.GetRibbonWidth(problemSpec);
            return SolverMaker.Solve(solver, ribbonWidth.HasValue ? ribbonWidth.Value : 1, originality);
        }
    }
}