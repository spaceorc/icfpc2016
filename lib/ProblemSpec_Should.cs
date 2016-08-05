using System;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class ProblemSpec_Should
	{
		[Test]
		public void BeParsable()
		{
			var input = @"1
4
0,0
1,0
1/2,1/2
0,1/2
5
0,0 1,0
1,0 1/2,1/2
1/2,1/2 0,1/2
0,1/2 0,0
0,0 1/2,1/2";
			ProblemSpec spec = ProblemSpec.Parse(input);
			Console.WriteLine(spec);
			spec.Polygons.Length.Should().Be(1);
			spec.Segments.Length.Should().Be(5);
			spec.ToString().Should().Be(input);
		}
	}
}