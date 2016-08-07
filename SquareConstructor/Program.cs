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
			var matrix = new SegmentsMatrix(10);
			List<bool> successes = new List<bool>();
			
			var problem =
				@"1
13
-1/42,0
1/42,0
5/102,6/119
7/18,2/7
22/75,46/75
170/819,413/702
29/210,143/210
0,97/168
-29/210,143/210
-170/819,413/702
-22/75,46/75
-7/18,2/7
-5/102,6/119
16
-1/42,0 -23/84,1/2
1/42,0 23/84,1/2
-1/42,0 -2/21,1/2
1/42,0 2/21,1/2
-1/42,0 1/42,0
-1/42,0 7/18,2/7
1/42,0 -7/18,2/7
-7/18,2/7 -22/75,46/75
7/18,2/7 22/75,46/75
-23/84,1/2 -29/210,143/210
23/84,1/2 29/210,143/210
-7/18,2/7 29/210,143/210
7/18,2/7 -29/210,143/210
-2/21,1/2 22/75,46/75
2/21,1/2 -22/75,46/75
-23/84,1/2 23/84,1/2
";

			var spec = ProblemSpec.Parse(problem);

			var solver = new ConstructorSolver(spec);
			solver.Work();

			var polygons = PolygonFinder.GetRealPolygons(spec);
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new PolygonsAndSegmentsForm();
            form.SetData(spec.Polygons, polygons.SelectMany(p => p.Segments).ToArray());
            Application.Run(form);
        }
	}
}
