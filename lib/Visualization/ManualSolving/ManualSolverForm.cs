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
			var copy = new ToolStripButton("Reflect copy", null, (sender, args) => Model.StartOperation(PendingOperationType.ReflectCopy));
			var move = new ToolStripButton("Reflect move", null, (sender, args) => Model.StartOperation(PendingOperationType.ReflectMove));
			var cancel = new ToolStripButton("Cancel", null, (sender, args) => Model.CancelPendingOperation());
			var menu = new ToolStrip(copy, move, cancel);
			WindowState = FormWindowState.Maximized;
			this.Controls.Add(menu);
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
			Model.UpdateHighlightedSegment(new Vector(x, y));
			Invalidate();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			Model.SelectSegment();
			Invalidate();
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
				Model.Segments, Model.HighlightedSegmentIndex, Model.SelectedSegmentIndices,
				Model.Shift + new Vector(margin, margin));
		}
	}
}