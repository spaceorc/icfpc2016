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
		public Rational Size;

		public SegmentsMatrix(int size)
		{
			Matrix = new SegmentMatrixNode[size, size];
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					Matrix[i, j] = new SegmentMatrixNode();
				}
			}

			Size = Rational.Parse("1") / size;
		}

		public bool TryAddPolygon(Polygon polygon)
		{
			if (polygon.Vertices.Any(vertex => vertex.X < 0 || vertex.X > 1 || vertex.Y < 0 || vertex.Y > 1))
			{
				return false;
			}

			int lastAddedSegment = 0;
			for (; lastAddedSegment < polygon.Segments.Length; lastAddedSegment++)
			{
				if(!AddSegment(polygon.Segments[lastAddedSegment], polygon))
					break;
			}

			if (lastAddedSegment != polygon.Segments.Length)
			{
				for (int i = 0; i <= lastAddedSegment; i++)
				{
					RemoveSegment(polygon.Segments[i], polygon);
				}
			}

			return lastAddedSegment == polygon.Segments.Length;
		}

		public void RemovePolygon(Polygon polygon)
		{
			polygon.Segments.ForEach(segment => RemoveSegment(segment, polygon));
		}

		private bool AddSegment(Segment segment, Polygon polygon)
		{
			bool success = true;
			DoItToLine(segment, node =>
			{
				if (!success)
					return;
				if (!node.Contains(segment, polygon) && (node.HasIntersections(segment)))
					success = false;
				else
					node.Add(segment, polygon);
			});
			return success;
		}

		private void RemoveSegment(Segment segment, Polygon polygon)
		{
			DoItToLine(segment, node => node.Remove(segment, polygon));
		}

		private void DoItToLine(Segment segment, Action<SegmentMatrixNode> doIt)
		{
			var start = segment.Start / Size;
			var end = segment.End / Size;

			var matrixSegment = new Segment(start, end);

			var minX = (start.X < end.X ? start.X : end.X).ToInt();
			var maxX = (start.X < end.X ? end.X : start.X).ToInt();
			var minY = (start.Y < end.Y ? start.Y : end.Y).ToInt();
			var maxY = (start.Y < end.Y ? end.Y : start.Y).ToInt();

			for (int i = minX; i <= maxX; i++)
			{
				var y = matrixSegment.GetYIntersect(i);
				if(y == null)
					continue;
				var intY = y.Value.ToInt();
				DoItToNode(i, intY, doIt);
				DoItToNode(i-1, intY, doIt);
				if (y.Value.IsInt())
				{
					DoItToNode(i, intY - 1, doIt);
					DoItToNode(i - 1, intY - 1, doIt);
				}
			}

			for (int j = minY; j <= maxY; j++)
			{
				var x = matrixSegment.GetXIntersect(j);
				if (x == null)
					continue;
				var intX = x.Value.ToInt();
				DoItToNode(intX, j, doIt);
				DoItToNode(intX, j-1, doIt);

				if (x.Value.IsInt())
				{
					DoItToNode(intX - 1, j, doIt);
					DoItToNode(intX - 1, j - 1, doIt);
				}
			}
		}

		private void DoItToNode(int x, int y, Action<SegmentMatrixNode> doIt)
		{
			if(x < 0 || x >= Matrix.GetLength(0) || y < 0 || y >= Matrix.GetLength(1))
				return;

			doIt(Matrix[x, y]);
		}
	}

	class SegmentMatrixNode
	{
		public HashSet<Tuple<Segment, Polygon>> Segments = new HashSet<Tuple<Segment, Polygon>>(); 

		public bool Add(Segment segment, Polygon polygon)
		{
			return Segments.Add(Tuple.Create(segment, polygon));
		}

		public void Remove(Segment segment, Polygon polygon)
		{
			Segments.Remove(Tuple.Create(segment, polygon));
		}

		public bool Contains(Segment segment, Polygon polygon)
		{
			return Segments.Contains(Tuple.Create(segment, polygon));
		}

		public bool HasIntersections(Segment segment)
		{
			return Segments.Any(pair =>
			{
				var intersection = segment.GetIntersection(pair.Item1);
				if (intersection == null)
				{
					return CheckIfBadCollinear(segment, pair.Item1);
				}
				if ((intersection.Equals(segment.Start) || intersection.Equals(segment.End)) && (intersection.Equals(pair.Item1.Start) || intersection.Equals(pair.Item1.End)))
					return false;
				return true;
			});
		}

		private bool CheckIfBadCollinear(Segment segment1, Segment segment2)
		{
			if (!segment1.AreSegmentsOnSameLine(segment2))
				return false;
			return IsBetweenOnLine(segment1.Start, segment2.Start, segment1.End)
				   || IsBetweenOnLine(segment1.Start, segment2.End, segment1.End)
				   || IsBetweenOnLine(segment2.Start, segment1.Start, segment2.End)
				   || IsBetweenOnLine(segment2.Start, segment1.End, segment2.End);
		}

		private static bool IsBetweenOnLine(Vector a, Vector x, Vector b)
		{
			return (a - x).ScalarProd(b - x) < 0;
		}

	}
}
