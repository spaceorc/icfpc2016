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
			var segment = new Segment(new Point(0, 0), new Point(10, 0));
			Console.WriteLine(segment.GetIntersection(new Segment(new Point(0, 0), new Point(10, 1))));
			Console.WriteLine(segment.GetIntersection(new Segment(new Point(10, 1), new Point(0, 0))));
			Console.WriteLine(segment.GetIntersection(new Segment(new Point(10, 1), new Point(10, 0))));
			Console.WriteLine(segment.GetIntersection(new Segment(new Point(10, 0), new Point(10, 1))));
			Console.WriteLine(segment.GetIntersection(new Segment(new Point(11, 0), new Point(10, 1))));
		}
	}
}
