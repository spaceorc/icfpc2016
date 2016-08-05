using ApprovalTests;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class SolutionSpec_Should
	{
		[Test]
		public void Writable()
		{
			var pts = new[] {Point.Parse("0,0"), Point.Parse("0,1"), Point.Parse("1,1"), Point.Parse("1,0") };
			var solutionSpec = new SolutionSpec(pts, new[] { new Facet(0, 1, 2, 3) }, pts);
			Approvals.Verify(solutionSpec);
		}
	}
}