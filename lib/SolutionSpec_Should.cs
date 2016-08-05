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
			var pts = new[] {Vector.Parse("0,0"), Vector.Parse("0,1"), Vector.Parse("1,1"), Vector.Parse("1,0") };
			var solutionSpec = new SolutionSpec(pts, new[] { new Facet(0, 1, 2, 3) }, pts);
			Approvals.Verify(solutionSpec);
		}
	}
}