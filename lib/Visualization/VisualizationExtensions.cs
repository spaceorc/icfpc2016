using System;
using System.Windows.Forms;

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

		public static Form CreateVisualizerForm(this SolutionSpec solution, bool dest = false)
		{
			var form = new Form();
			Painter painter = new Painter();
			if (dest)
				form.Paint +=
					(sender, args) => painter.PaintDest(args.Graphics, Math.Min(form.ClientSize.Height, form.ClientSize.Width), solution);
			else
				form.Paint +=
				(sender, args) => painter.Paint(args.Graphics, Math.Min(form.ClientSize.Height, form.ClientSize.Width), solution);
			form.Text = "Problem";
			return form;
		}
	}
}