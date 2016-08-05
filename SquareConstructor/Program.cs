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
			var vectors = new Point[]
			{
				new Point(0, 1),
				new Point(1, 1),
				new Point(-1, 1),
				new Point(0, -1),
				new Point(0, 1),
			};

			var results = vectors.Take(vectors.Length - 1).Select((v, i) => GeometryExtensions.GetAngleMeasure(v, vectors[i+1])).ToArray();
			Console.WriteLine();

		}
	}
}
