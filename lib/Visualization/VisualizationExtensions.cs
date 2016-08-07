using System;
using System.Linq;
using System.Windows.Forms;
using lib.Api;

namespace lib
{
	public static class VisualizationExtensions
	{
		public static Form CreateVisualizerForm(this ProblemSpec problem)
		{
			var form = new Form();
			Painter painter = new Painter();
			form.Paint +=
				(sender, args) => painter.Paint(args.Graphics, Math.Min(form.ClientSize.Height, form.ClientSize.Width), problem);
			form.Text = "Problem";
			return form;
		}

		public static Form CreateVisualizerForm(this SolutionSpec solution, int problemId = -1)
		{
			var form = new Form();
			var split = new SplitContainer();
			split.Dock = DockStyle.Fill;
			var menu = new MenuStrip();
			if (problemId >= 0)
			{
				menu.Items.Add("submit").Click += (sender, args) => PostSolution(problemId, solution);
			}
			form.Controls.Add(split);
			form.Controls.Add(menu);
			split.Panel1.DoubleClick += (sender, args) =>
			{
				CopyToClipboard(solution);
			};
			split.Panel2.DoubleClick += (sender, args) =>
			{
				CopyToClipboard(solution);
			};
			form.WindowState = FormWindowState.Maximized;
			Painter painter = new Painter();
			split.Panel1.Paint +=
					(sender, args) => painter.Paint(args.Graphics, Math.Min(split.Panel1.ClientSize.Height, split.Panel1.ClientSize.Width), solution);
			split.Panel2.Paint +=
				(sender, args) => painter.PaintDest(args.Graphics, Math.Min(split.Panel2.ClientSize.Height, split.Panel2.ClientSize.Width), solution);
			split.Resize += (sender, args) => split.Invalidate();
			form.Text = "Solution (Doule click to copy solution to clipboard) " + problemId;
			return form;
		}

		private static void PostSolution(int problemId, SolutionSpec solution)
		{
			try
			{
				var res = ProblemsSender.Post(solution, problemId);
				if (res == 1.0)
				{
					var repo = new ProblemsRepo();
					var problemSpec = repo.Get(problemId);
					var problemSpecStr = problemSpec.ToString();
					var toSend = repo.GetAllNotSolvedPerfectly().Where(p => p.ToString() == problemSpecStr).ToList();
					foreach (var sameProb in toSend)
					{
						res = Math.Min(res, ProblemsSender.Post(solution, sameProb.id));
					}
					MessageBox.Show($"Resemblance = 1.0. {toSend.Count} same problem. min resemblence = {res}");
				}
				else
					MessageBox.Show("Resemblance = " + res + " no same problems");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), e.Message);
			}
		}

		private static void CopyToClipboard(SolutionSpec solution)
		{
			Clipboard.SetText(solution.ToString());
			MessageBox.Show("Solution have copied to clipboard");
		}
	}
}