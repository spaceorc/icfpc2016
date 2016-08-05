using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace lib
{
	public class Painter
	{
		static string[] ColourValues = new string[] {
		"FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000",
		"800000", "008000", "000080", "808000", "800080", "008080", "808080",
		"C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0",
		"400000", "004000", "000040", "404000", "400040", "004040", "404040",
		"200000", "002000", "000020", "202000", "200020", "002020", "202020",
		"600000", "006000", "000060", "606000", "600060", "006060", "606060",
		"A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
		"E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0",
	};
		public void Paint(Graphics g, int size, ProblemSpec spec)
		{
			g.FillRectangle(Brushes.Beige, new RectangleF(0, 0, size, size));
			var spec2 = spec.MoveToOrigin();
			g.ScaleTransform(size/1.5f, size/1.5f);
			var i = 0;
			foreach (var polygon in spec2.Polygons)
			{
				Color color = ColorTranslator.FromHtml("#" + ColourValues[i++]);
				PaintPolygon(g, color, polygon);
			}

            int cyan = 0;

			foreach (var segment in spec2.Segments)
			{
                var color = Color.Black;

                if (Arithmetic.IsSquare(segment.QuadratOfLength))
                {
                    color = Color.FromArgb(0, 255 - cyan, 255 - cyan);
                    cyan = cyan += 30;
                    if (cyan > 150) cyan = 0;
                }

                PaintSegment(g, color, segment);
			}
		}

        
		public void PaintSegment(Graphics g, Color color, Segment segment)
		{
			g.DrawLine(new Pen(color, 0.01f), segment.Start.X, segment.Start.Y, segment.End.X, segment.End.Y);
		}

		public void PaintPolygon(Graphics g, Color color, Polygon polygon)
		{
			var ps = polygon.Vertices.Select(v => new PointF(v.X, v.Y)).ToArray();
			g.FillPolygon(new SolidBrush(color), ps);
		}
	}
}