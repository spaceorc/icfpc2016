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

        public static void SolveAndShow(int taskNumber, Rational otherSide)
        {
            var problemSpec = new ProblemsRepo().Get(taskNumber);

            var solver = SolverMaker.Solve(SolverMaker.CreateSolver(problemSpec), otherSide);
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
            return ProblemsSender.Post(problemSpec, solutionSpec);
        }

        private static SolutionSpec Solve(ProblemSpec problemSpec, double originality)
        {
            var solver = SolverMaker.CreateSolver(problemSpec);
            var ribbonWidth = RibbonIndicator.GetRibbonWidth(problemSpec);
            if (ribbonWidth.HasValue)
            {
                var ribbonSolver = SolverMaker.Solve(solver, ribbonWidth.Value, originality);
                return ribbonSolver != null
                    ? SolutionSpecBuilder.BuildSolutionByRibbonGraph(solver.Projection)
                    : null;
            }
            var simpleSolver = SolverMaker.Solve(solver, 1, originality);
            return simpleSolver != null
                ? SolutionSpecBuilder.BuildSolutionByGraph(solver.Projection)
                : null;
        }
    }
}