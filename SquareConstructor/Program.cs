using System;
using System.Collections.Generic;
using System.IO;
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
			var problem = File.ReadAllText("../../../problems/008.spec.txt");

			var spec = ProblemSpec.Parse(problem);

			var solver = new ConstructorSolver(spec);
			var solution = solver.Work();
			
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new PolygonsAndSegmentsForm();
            form.SetData(solution.Polygons, new Segment[0]);
            Application.Run(form);
        }
	}
}
