using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using lib;

namespace SquareConstructor
{
	class Program
	{
		static void Main(string[] args)
		{
			var problem =
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
0,1 174/1001,1";

			var spec = ProblemSpec.Parse(problem);

			var polygons = PolygonFinder.GetRealPolygons(spec);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new PolygonsAndSegmentsForm();
            form.SetData(null, null);
            Application.Run(form);
        }
	}
}
