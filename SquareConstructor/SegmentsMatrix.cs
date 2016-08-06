using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace SquareConstructor
{
	class SegmentsMatrix
	{
		public SegmentMatrixNode[,] Matrix;

		public SegmentsMatrix(int size)
		{
			Matrix = new SegmentMatrixNode[size, size];
		}
	}

	class SegmentMatrixNode
	{
		public Dictionary<Segment, Polygon> Polygons = new Dictionary<Segment, Polygon>();

		public void Add(Segment segment, Polygon polygon)
		{
			Polygons.Add(segment, polygon);
		}

		public void Remove(Segment segment)
		{

		}
	}
}
