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
			//copy.ShortcutKeys = Keys.Q;
			var move = new ToolStripMenuItem("Reflect move", null, (sender, args) => ChangeModel(Model.StartOperation(PendingOperationType.ReflectMove)));
			//move.ShortcutKeys = Keys.W;
			var cancel = new ToolStripMenuItem("Cancel", null, (sender, args) => ChangeModel(Model.CancelPendingOperation()));
			//cancel.ShortcutKeys = Keys.Escape;
			var undo = new ToolStripMenuItem("Undo", null, (sender, args) => Undo());
			undo.ShortcutKeys = Keys.Z | Keys.Control;
			var redo = new ToolStripMenuItem("Redo", null, (sender, args) => Redo());
			redo.ShortcutKeys = Keys.Z | Keys.Control | Keys.Shift;
			var menu = new ToolStrip(copy, move, cancel, undo, redo);
			WindowState = FormWindowState.Maximized;
			this.Controls.Add(menu);
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

		public Rational Margin = new Rational(1, 1);

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