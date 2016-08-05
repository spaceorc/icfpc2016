using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace lib
{
	public class VisualizerForm : Form
	{
		private readonly string problemsDir;
		private readonly Func<ProblemSpec, SolutionSpec> solve;
		private Painter painter = new Painter();
		private ProblemSpec problem;
		private SolutionSpec solution;
		private ListBox list;
		private Panel problemPanel;
		private Panel solutionPanel;
		private double score;
		private SplitContainer splitContainer;

		public VisualizerForm(string problemsDir, Func<ProblemSpec, SolutionSpec> solve = null)
		{
			this.problemsDir = problemsDir;
			this.solve = solve;
			list = new ListBox();
			list.Dock = DockStyle.Left;
			list.BringToFront();
			list.Items.AddRange(GetProblems(problemsDir));
			list.SelectedValueChanged += ListOnSelectedValueChanged;
			splitContainer = new SplitContainer()
			{
				Dock = DockStyle.Fill,
			};
			splitContainer.Panel1.Paint += (sender, args) => PaintProblem(args.Graphics, splitContainer.Panel1.ClientSize);
			splitContainer.Panel2.Paint += (sender, args) => PaintSpec(args.Graphics, splitContainer.Panel2.ClientSize);
			splitContainer.SplitterDistance = splitContainer.ClientSize.Width/2;
			Size = new Size(800, 600);
			Controls.Add(splitContainer);
			Controls.Add(list);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Text = problemsDir;
			if (list.Items.Count > 0)
				list.SelectedIndex = 0;
		}

		private void ListOnSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			problem = ProblemSpec.Parse(File.ReadAllText(Path.Combine(problemsDir, (string) list.SelectedItem)));
			solution = solve?.Invoke(problem);
			if (solution != null)
			{
				score = SolutionEvaluator.EvaluateX(problem, solution, 100);
				Text = $"Score: {score}";
			}
			splitContainer.Panel1.Invalidate();
			splitContainer.Panel2.Invalidate();
		}

		private object[] GetProblems(string dir)
		{
			return Directory.EnumerateFiles(dir, "*.spec.txt").Select(Path.GetFileName).Cast<object>().ToArray();
		}

		private void PaintProblem(Graphics graphics, Size clientSize)
		{
			if (problem != null)
			{
				painter.Paint(graphics, Math.Min(clientSize.Height, clientSize.Width), problem);
			}
		}

		private void PaintSpec(Graphics graphics, Size clientSize)
		{
			if (solution != null)
			{
				painter.Paint(graphics, Math.Min(clientSize.Height, clientSize.Width), solution);
			}
		}
	}

	[TestFixture]
	public class VisualizerForm_Should
	{
		[Test]
		public void DoSomething_WhenSomething()
		{
			new VisualizerForm(@"c:\work\icfpc2016\problems", p => new ImperfectSolver().SolveMovingInitialSquare(p)).ShowDialog();
		}
	}
}