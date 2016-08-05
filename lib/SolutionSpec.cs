using System;
using System.Linq;
using System.Text;
using ApprovalTests.Reporters;

namespace lib
{
	public class SolutionSpec
	{
		public readonly Vector[] SourcePoints;
		public readonly Facet[] Facets;
		public readonly Vector[] DestPoints;

		public SolutionSpec(Vector[] sourcePoints, Facet[] facets, Vector[] destPoints)
		{
			if (sourcePoints.Length != destPoints.Length)
				throw new ArgumentException();
			SourcePoints = sourcePoints;
			Facets = facets;
			DestPoints = destPoints;
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
	}
}