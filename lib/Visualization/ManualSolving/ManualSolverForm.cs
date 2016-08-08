using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace lib.Visualization.ManualSolving
{
	public class ManualSolverForm : Form
	{
		private ManualSolverModel Model;

		public ManualSolverForm(ProblemSpec problem)
		{
			Model = new ManualSolverModel(problem);
			var copy = new ToolStripMenuItem("Reflect copy", null, (sender, args) => ChangeModel(Model.StartOperation(PendingOperationType.ReflectCopy)));
			copy.ShortcutKeys = Keys.Q | Keys.Control;
			var move = new ToolStripMenuItem("Reflect move", null, (sender, args) => ChangeModel(Model.StartOperation(PendingOperationType.ReflectMove)));
			move.ShortcutKeys = Keys.W | Keys.Control;
			var cancel = new ToolStripMenuItem("Cancel", null, (sender, args) => ChangeModel(Model.CancelPendingOperation()));
			var undo = new ToolStripMenuItem("Undo", null, (sender, args) => Undo());
			undo.ShortcutKeys = Keys.Z | Keys.Control;
			var redo = new ToolStripMenuItem("Redo", null, (sender, args) => Redo());
			redo.ShortcutKeys = Keys.Z | Keys.Control | Keys.Shift;
			var border = new ToolStripMenuItem("MarkAsBorder", null, (sender, args) => ChangeModel(Model.MarkAsBorder()));
			var noborder = new ToolStripMenuItem("MarkAsNOTBorder", null, (sender, args) => ChangeModel(Model.MarkAsNoBorder()));
			var solve = new ToolStripMenuItem("Solve", null, SolveClick);
			var selectAll = new ToolStripMenuItem("Select All", null, (sender, args) => ChangeModel(Model.SelectAll()));
			selectAll.ShortcutKeys = Keys.A | Keys.Control;
			var menu = new ToolStrip(selectAll, copy, move, cancel, undo, redo, border, noborder, solve);
			WindowState = FormWindowState.Maximized;
			this.Controls.Add(menu);
		}

		private void SolveClick(object sender, EventArgs e)
		{
			if (!Model.SelectedSegmentIndices.Any())
				MessageBox.Show("Выдели ребра выпуклого многоугольника, который нужно решить.");
			else
			{
				var sols = Model.SolveConvex();
				foreach (var sol in sols)
					sol.CreateVisualizerForm(Model.Problem.id).Show(this);
				//MessageBox.Show("Решение скопировано в буфер. К решению выпуклого применены все сделанные в редакторе фолды в обратном порядке. Можно пробовать сабмитить.");
			}
		}

		private void Undo()
		{
			if (!done.Any()) return;
			undone.Push(Model);
			Model = done.Pop();
			Invalidate();
		}

		private void Redo()
		{
			if (!undone.Any()) return;
			done.Push(Model);
			Model = undone.Pop();
			Invalidate();
		}

		private Stack<ManualSolverModel> done = new Stack<ManualSolverModel>();
		private Stack<ManualSolverModel> undone = new Stack<ManualSolverModel>();

		private void ChangeModel(ManualSolverModel newModel)
		{
			if (newModel == Model) return;
			if (Model != null) done.Push(Model);
			undone.Clear();
			Model = newModel;
			Invalidate();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.DoubleBuffered = true;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			var scaleFactor = GetScaleFactor();
			var y = new Rational(e.Y - ClientRectangle.Top, 1)/scaleFactor - Margin;
			var x = new Rational(e.X - ClientRectangle.Left, 1)/scaleFactor - Margin;
			Model.UpdateHighlightedSegment(new Vector(x, y) - Model.Shift);
			Invalidate();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			ChangeModel(Model.SelectSegment());
		}

		public new Rational Margin = new Rational(1, 1);

		public Rational GetScaleFactor()
		{
			var size = Math.Min(ClientSize.Height, ClientSize.Width);
			return new Rational(size, 1)/(Margin*2 + 1);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Text = Model.PendingOperation.ToString();
			var g = e.Graphics;
			Rational margin = Margin;
			var scaleFactor = GetScaleFactor();
			g.ScaleTransform(scaleFactor, scaleFactor);
			new Painter().PaintSkeleton(g,
				Model.Segments.ToArray(), Model.HighlightedSegmentIndex, Model.SelectedSegmentIndices,
				Model.Shift + new Vector(margin, margin));
		}
	}
}