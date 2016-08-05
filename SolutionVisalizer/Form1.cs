using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using lib;
using SquareConstructor;

namespace SolutionVisalizer
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			
		}


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

		private void Form1_Paint(object sender, PaintEventArgs e)
		{

			var spec =
@"1
10
-4267/58289,-11692/524601
469/1672,36437/52668
174/1001,1
0,1
0,23096929/23647932
-38493988/94603047,30998098/31534349
-96477/233156,156742/291445
-27719/58289,28517/58289
-751181/1806959,746582/1806959
-39957373/94603047,-532478/31534349
11
-4267/58289,-11692/524601 -27719/58289,28517/58289
-4267/58289,-11692/524601 -39957373/94603047,-532478/31534349
0,52/63 0,1
-39957373/94603047,-532478/31534349 -38493988/94603047,30998098/31534349
-4267/58289,-11692/524601 469/1672,36437/52668
469/1672,36437/52668 174/1001,1
0,52/63 9380/51293,286093/293769
-27719/58289,28517/58289 174/1001,1
469/1672,36437/52668 0,52/63
9380/51293,286093/293769 -38493988/94603047,30998098/31534349
0,1 174/1001,1
";

			var problem = ProblemSpec.Parse(spec);
			var polygons = PolygonFinder.GetRealPolygons(problem);
			
			var painter = new Painter();
			
			problem.Segments = polygons[8].Segments;
			painter.Paint(e.Graphics, e.ClipRectangle.Height, problem);
			Update();
			
		}
	}
}
