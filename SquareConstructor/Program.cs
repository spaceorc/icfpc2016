using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace SquareConstructor
{
	class Program
	{
		static void Main(string[] args)
		{
			Segment s = new Segment(new Vector(0, 0), new Vector(0, 5));
			Segment d = new Segment(new Vector(1, 2), new Vector(5, 5));

			var matrix = TransposeOperator.ConstructOperator(s, d);
			Console.WriteLine();
		}
	}
}
