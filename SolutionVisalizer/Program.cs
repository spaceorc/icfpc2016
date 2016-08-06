using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using lib;
using Runner;
using Path = System.IO.Path;

namespace SolutionVisalizer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new VisualizerForm(GetProblemsDir(), Solve));
		}

		private static SolutionSpec Solve(ProblemSpec arg)
		{
			//return GraphExt.Solve(arg);
			return new ImperfectSolver().SolveMovingInitialSquare(arg);
		}

		private static string GetProblemsDir()
		{
			return Path.GetFullPath("problems");
		}
	}
}