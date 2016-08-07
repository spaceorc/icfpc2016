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
		private SolutionSpec solution;
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

				var solve = new ToolStripButton("Solve", null, SolveClick);
			var menu = new ToolStrip(sortByExpectedScore, sortById, solve);
				list = new ListBox();
				list.Width = 300;
				list.Dock = DockStyle.Left;
				list.BringToFront();
				snapshotJson = repo.GetSnapshot(api);
				problemsJson = snapshotJson.Problems.ToDictionary(p => p.Id, p => p);

				list.Items.AddRange(GetItems(false));
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
				Controls.Add(menu);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}



        private void SolveClick(object sender, EventArgs e)
		{
			var res = ProblemsSender.TrySolveAndSend(problem);
			MessageBox.Show($"resemblance = {res}");
		}

		private object[] GetItems(bool sortScore)
		{
			var allItems = repo.GetAll().Select(CreateItem);
			if (!sortScore) return allItems.Cast<object>().ToArray();
			return allItems.Where(p => !p.IsSolved).OrderByDescending(p => p.ExpectedScore).Cast<object>().ToArray();
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
				var json = JsonConvert.DeserializeObject<PostResponseJson>(resp);
				res.OurResemblance = json.resemblance;
			}
			if (problemsJson.ContainsKey(problem.id))
			{
				res.json = problemsJson[problem.id];
				res.ExpectedScore = problemsJson[problem.id].ExpectedScore();
			}
			return res;
		}

		private void SortByExpectedScoreClick(object sender, EventArgs eventArgs)
		{
			list.Items.Clear();
			list.Items.AddRange(GetItems(true));
		}

        private void SortByIdClick(object sender, EventArgs eventArgs)
        {
            list.Items.Clear();
            list.Items.AddRange(GetItems(false));
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
			problem = ((ProblemListItem) list.SelectedItem).Spec;
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

				    var gist = RibbonGist(problem);
                    output.Items.Clear();
                    foreach (var g in gist)
                    {
                        output.Items.Add(string.Format("{0}\t{1}", g.Key, g.Value));
                    }
				}
				catch
				{
					Text = "ERROR";
				}
			}
		}

        public static KeyValuePair<Rational, long>[] RibbonGist(ProblemSpec spec)
        {
            var result = new Dictionary<Rational, long>();
            var solver = SolverMaker.CreateSolver(spec);
            var rationalSegments = solver.AllSegments
                .Where(s => Arithmetic.IsSquare(s.QuadratOfLength))
                .ToArray();
            var rationalVectors = new HashSet<Vector>();
            foreach (var segment in rationalSegments)
            {
                rationalVectors.Add(segment.Start);
                rationalVectors.Add(segment.End);
            }
            foreach (var vector in rationalVectors)
            {
                foreach (var segment in rationalSegments)
                {
                    var d = segment.Distance2To(vector);
                    if (d == 0)
                        continue;
                    d = d.Reduce();
                    //if (d.Numerator != 1)
                    //    continue;

                    if (!result.ContainsKey(d))
                        result[d] = 0;
                    result[d]++;
                }
            }
            return result.OrderByDescending(p => p.Value).Take(10).ToArray();
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