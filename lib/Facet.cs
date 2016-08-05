namespace lib
{
	public class Facet
	{
		public readonly int[] Vertices;

		public Facet(params int[] vertices)
		{
			Vertices = vertices;
		}

		public override string ToString()
		{
			return $"{Vertices.Length} {Vertices.StrJoin(" ")}";
		}
	}
}