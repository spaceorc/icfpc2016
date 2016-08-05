using System;

namespace lib
{
	public class Segment
	{
		public readonly Point Start, End;

		public Segment(Point start, Point end)
		{
			Start = start;
			End = end;
		}

		public static Segment Parse(string s)
		{
			var parts = s.Split(' ');
			if (parts.Length != 2) throw new FormatException(s);
			return new Segment(Point.Parse(parts[0]), Point.Parse(parts[1]));
		}

		public override string ToString()
		{
			return $"{Start} {End}";
		}
	}
}