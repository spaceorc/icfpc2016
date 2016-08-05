using System;
using System.Diagnostics.Contracts;

namespace lib
{
	public class Segment
	{
		public readonly Vector Start, End;

		public Segment(Vector start, Vector end)
		{
			Start = start;
			End = end;
		}

        public Rational QuadratOfLength
        {
            get
            {
                var result= (End.X - Start.X) * (End.X - Start.X) +
                (End.Y - Start.Y) * (End.Y - Start.Y);

                result.Reduce();
                return result;
            }
        }

		public static implicit operator Segment(string s)
		{
			return Parse(s);
		}
        

		public static Segment Parse(string s)
		{
			var parts = s.Split(' ');
			if (parts.Length != 2) throw new FormatException(s);
			return new Segment(Vector.Parse(parts[0]), Vector.Parse(parts[1]));
		}

		public Segment Reflect(Segment mirror)
		{
			return new Segment(Start.Reflect(mirror), End.Reflect(mirror));
		}

		public override string ToString()
		{
			return $"{Start} {End}";
		}
		[Pure]
		public Segment Move(Rational shiftX, Rational shiftY)
		{
			return new Segment(Start.Move(shiftX, shiftY), End.Move(shiftX, shiftY));
		}
	}
}