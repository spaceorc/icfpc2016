using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using lib;
using lib.Api;
using lib.ProjectionSolver;
using lib.Visualization.ManualSolving;
using Newtonsoft.Json;
using NUnit.Framework;

namespace lib
{
	public class ProblemListItem
	{
		public int Id;
		public ProblemSpec Spec;
		public double ExpectedScore;
		public double OurResemblance;
		public ProblemJson json;
		public string Owner => json?.Owner ?? "";
		public bool IsSolved => OurResemblance == 1.0;

		public override string ToString()
		{
			return $"{Id} Exp {ExpectedScore:#} {(IsSolved ? "SOLVED" : OurResemblance.ToString("#.###"))} {Owner}";
		}
	}

	public class VisualizerForm : Form
	{
		private readonly ProblemsRepo repo = new ProblemsRepo();
		private readonly ApiClient api = new ApiClient();
		private Painter painter = new Painter();
		private ProblemSpec problem;
		private ListBox list;
		private Panel problemPanel;
		private SnapshotJson snapshotJson;
		private Dictionary<int, ProblemJson> problemsJson;
		private ListBox output;



		public VisualizerForm()
		{
			try
			{
				var sortByExpectedScore = new ToolStripButton("SortByScore", null, SortByExpectedScoreClick);
				sortByExpectedScore.CheckOnClick = true;

				var sortById = new ToolStripButton("SortById", null, SortByIdClick);
				sortById.CheckOnClick = true;
				var sortByOwner = new ToolStripButton("SortByOwner", null, SortByOwnerClick);
				sortByOwner.CheckOnClick = true;

				var solve = new ToolStripButton("Solve", null, SolveClick);
				var menu = new ToolStrip(sortByExpectedScore, sortById, sortByOwner, solve);
				list = new ListBox();
				list.Width = 300;
				list.Dock = DockStyle.Left;
				list.BringToFront();

				output = new ListBox();
				output.Dock = DockStyle.Bottom;

				snapshotJson = repo.GetSnapshot(api);
				problemsJson = snapshotJson.Problems.ToDictionary(p => p.Id, p => p);

				list.Items.AddRange(GetItems(x => x));
				list.SelectedValueChanged += ListOnSelectedValueChanged;
				list.DoubleClick += ListOnDoubleClick;

				problemPanel = new Panel()
				{
					Dock = DockStyle.Fill,
				};

				problemPanel.Paint += (sender, args) => PaintProblem(args.Graphics, problemPanel.ClientSize);

				Size = new Size(800, 600);
				Controls.Add(problemPanel);
				Controls.Add(output);
				Controls.Add(list);
				Controls.Add(menu);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		private void SolveClick(object sender, EventArgs e)
		{
			var res = ProblemsSender.TrySolveAndSend(problem);
			MessageBox.Show($"resemblance = {res}");
		}

		private object[] GetItems(Func<IEnumerable<ProblemListItem>, IEnumerable<ProblemListItem>> sortAndFilter)
		{
			IEnumerable<ProblemListItem> allItems = repo.GetAll().Select(CreateItem);
			return sortAndFilter(allItems).Cast<object>().ToArray();
		}

		private ProblemListItem CreateItem(ProblemSpec problem)
		{
			var res = new ProblemListItem()
			{
				Id = problem.id,
				Spec = problem
			};
			var resp = repo.FindResponse(problem.id);
			if (resp != null)
			{
				var json = Read(resp);
				res.OurResemblance = json.resemblance;
			}
			if (problemsJson.ContainsKey(problem.id))
			{
				res.json = problemsJson[problem.id];
				res.ExpectedScore = problemsJson[problem.id].ExpectedScore();
			}
			return res;
		}

		private static PostResponseJson Read(string resp)
		{
			try
			{
				return JsonConvert.DeserializeObject<PostResponseJson>(resp);
			}
			catch (Exception e)
			{
				throw new Exception($"Responce Format Error: {resp}", e);
			}
		}

		private void SortByExpectedScoreClick(object sender, EventArgs eventArgs)
		{
			SortBy(allItems => allItems.Where(p => !p.IsSolved).OrderByDescending(p => p.ExpectedScore));
		}

		private void SortByOwnerClick(object sender, EventArgs e)
		{
			SortBy(x => x.OrderByDescending(p => p.Owner));
		}

		private void SortBy(Func<IEnumerable<ProblemListItem>, IEnumerable<ProblemListItem>> sortAndFilter)
		{
			list.Items.Clear();
			list.Items.AddRange(GetItems(sortAndFilter));
		}

		private void SortByIdClick(object sender, EventArgs eventArgs)
		{
			list.Items.Clear();
			list.Items.AddRange(GetItems(x => x));
		}

		private void ListOnDoubleClick(object sender, EventArgs eventArgs)
		{
			new ManualSolverForm(problem).Show(this);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (list.Items.Count > 0)
				list.SelectedIndex = 0;
		}

		private void ListOnSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			problem = ((ProblemListItem)list.SelectedItem).Spec;
			problemPanel.Invalidate();
		}

		private void PaintProblem(Graphics graphics, Size clientSize)
		{
			if (problem != null)
			{
				try
				{

					painter.Paint(graphics, Math.Min(clientSize.Height, clientSize.Width), problem);
					Text = problem.id.ToString();

					//PaintRibbonGistToOutput();
				}
				catch
				{
					Text = "ERROR";
				}
			}
		}

		private void PaintRibbonGistToOutput()
		{
			var solver = SolverMaker.CreateSolver(problem);
			KeyValuePair<Rational, double>[] gist = RibbonIndicator.RibbonGist(solver);
			var r = RibbonIndicator.Indicate(gist);
			output.Items.Clear();
			if (r.HasValue)
			{
				output.Items.Add("RIBBON!");
			}
			foreach (var g in gist)
			{
				output.Items.Add(string.Format("{0}\t{1}", g.Key, g.Value));
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
			new VisualizerForm().ShowDialog();
		}
	}
}