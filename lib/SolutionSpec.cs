using System;
using System.Linq;
using System.Text;

namespace lib
{
	public class SolutionSpec
	{
		public readonly Vector[] SourcePoints;
		public readonly Facet[] Facets;
		public readonly Vector[] DestPoints;
		private static readonly Vector[] initialSquare = "0,0 1,0 1,1 0,1".ToPoints();

		public static SolutionSpec CreateTrivial(Func<Vector, Vector> transform = null)
		{
			return new SolutionSpec(initialSquare, new[] { new Facet(0, 1, 2, 3) }, initialSquare.Select(transform ?? (x => x)).ToArray());
		}
		public SolutionSpec(Vector[] sourcePoints, Facet[] facets, Vector[] destPoints)
		{
			if (sourcePoints.Length != destPoints.Length)
				throw new ArgumentException();
			SourcePoints = sourcePoints;
			Facets = facets;
			DestPoints = destPoints;
		}

		public Polygon[] Polygons => Facets.Select(FacetToPolygon).ToArray();
		public Polygon[] PolygonsDest => Facets.Select(FacetToPolygonDst).ToArray();

		private Polygon FacetToPolygon(Facet f)
		{
			return new Polygon(f.Vertices.Select(i => SourcePoints[i]).ToArray());
		}
		private Polygon FacetToPolygonDst(Facet f)
		{
			return new Polygon(f.Vertices.Select(i => DestPoints[i]).ToArray());
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SourcePoints.Length.ToString());
			sb.AppendLine(SourcePoints.StrJoin(Environment.NewLine));
			sb.AppendLine(Facets.Length.ToString());
			sb.AppendLine(Facets.StrJoin(Environment.NewLine));
			sb.Append(DestPoints.StrJoin(Environment.NewLine));
			return sb.ToString();
		}

		public bool ValidateFacetSquares()
		{
			Rational totalSquare = 0;
			foreach (var facet in Facets)
			{
				var sourcePolygon = new Polygon(facet.Vertices.Select(index => SourcePoints[index]).ToArray());
				var destPolygon = new Polygon(facet.Vertices.Select(index => DestPoints[index]).ToArray());
				var sourceSquare = sourcePolygon.GetUnsignedSquare();
				if (sourceSquare != destPolygon.GetUnsignedSquare())
					return false;

				totalSquare += sourceSquare;
			}
			return totalSquare == 1;
		}
	}
}