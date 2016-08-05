using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class Rational_Should
	{
		[Test]
		public void BeParsable_EvenIfBig()
		{
			var v = Rational.Parse("1267650600228229401496703205377/1267650600228229401496703205376");
			v.Numerator.ToString().Should().Be("1267650600228229401496703205377");
			v.Denomerator.ToString().Should().Be("1267650600228229401496703205376");
		}
	}
}