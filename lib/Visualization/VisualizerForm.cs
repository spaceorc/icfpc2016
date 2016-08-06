using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using lib.Visualization.ManualSolving;
using NUnit.Framework;

namespace lib
{
	public class VisualizerForm : Form
	{
		private readonly string problemsDir;
		private Painter painter = new Painter();
		private ProblemSpec problem;
		private SolutionSpec solution;
		private ListBox list;
		private Panel problemPanel;

		public VisualizerForm(string problemsDir)
		{
			this.problemsDir = problemsDir;
			list = new ListBox();
			list.Dock = DockStyle.Left;
			list.BringToFront();
			list.Items.AddRange(GetProblems(problemsDir));
			list.SelectedValueChanged += ListOnSelectedValueChanged;
			list.DoubleClick += ListOnDoubleClick;
			
			problemPanel = new Panel()
			{
				Dock = DockStyle.Fill,
			};

			problemPanel.Paint += (sender, args) => PaintProblem(args.Graphics, problemPanel.ClientSize);

			Size = new Size(800, 600);
			Controls.Add(problemPanel);
			Controls.Add(list);
		}

		private void ListOnDoubleClick(object sender, EventArgs eventArgs)
		{
			new ManualSolverForm(problem).Show(this);
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
			problemPanel.Invalidate();
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
	}

	[TestFixture]
	public class VisualizerForm_Should
	{
		[Test]
		[Explicit]
		public void DoSomething_WhenSomething()
		{
			new VisualizerForm(@"c:\work\icfpc2016\problems").ShowDialog();
		}
	}
}