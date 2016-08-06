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
			Segment s = new Segment(new Vector(-4, -10), new Vector(-7, -6));
			Segment d = new Segment(new Vector(1/2, 1/2), new Vector(1, 1));

			//var matrix = TransposeOperator.ConstructOperator(s, d);
			//Console.WriteLine();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new PolygonsAndSegmentsForm();
            form.SetData(null, null);
            Application.Run(form);
        }
	}
}
