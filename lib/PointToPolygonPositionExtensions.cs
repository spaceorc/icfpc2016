namespace lib
{
	public enum PointToPolygonPositionType
	{
		Boundary,
		Inside,
		Outside
	}

	public static class PointToPolygonPositionExtensions
	{
		public static PointToPolygonPositionType GetPositionToPolygon(this Point p, Polygon polygon)
		{
			var parity = true;
			for (int i = 0; i < polygon.Vertices.Length; i++)
			{
				var v1 = polygon.Vertices[i];
				var v2 = polygon.Vertices[(i + 1)%polygon.Vertices.Length];
				var segment = new Segment(v1, v2);
				switch (ClassifyEdge(p, segment))
				{
					case EdgeType.TOUCHING:
						return PointToPolygonPositionType.Boundary;
					case EdgeType.CROSSING:
						parity = !parity;
						break;
				}
			}
			return parity ? PointToPolygonPositionType.Outside : PointToPolygonPositionType.Inside;
		}

		private enum EdgeType
		{
			CROSSING,
			INESSENTIAL,
			TOUCHING
		}

		private static EdgeType ClassifyEdge(Point a, Segment e)
		{
			Point v = e.Start;
			Point w = e.End;
			switch (a.Classify(e))
			{
				case PointClassification.LEFT:
					return ((v.Y < a.Y) && (a.Y <= w.Y)) ? EdgeType.CROSSING : EdgeType.INESSENTIAL;
				case PointClassification.RIGHT:
					return ((w.Y < a.Y) && (a.Y <= v.Y)) ? EdgeType.CROSSING : EdgeType.INESSENTIAL;
				case PointClassification.BETWEEN:
				case PointClassification.ORIGIN:
				case PointClassification.DESTINATION:
					return EdgeType.TOUCHING;
				default:
					return EdgeType.INESSENTIAL;
			}
		}

		private enum PointClassification
		{
			LEFT,
			RIGHT,
			BEYOND,
			BEHIND,
			BETWEEN,
			ORIGIN,
			DESTINATION
		};

		private static PointClassification Classify(this Point p, Segment s)
		{
			var a = s.End - s.Start;
			var b = p - s.Start;
			double sa = a.X*b.Y - b.X*a.Y;
			if (sa > 0.0)
				return PointClassification.LEFT;
			if (sa < 0.0)
				return PointClassification.RIGHT;
			if ((a.X*b.X < 0.0) || (a.Y*b.Y < 0.0))
				return PointClassification.BEHIND;
			if (a.Length2 < b.Length2)
				return PointClassification.BEYOND;
			if (s.Start.Equals(p))
				return PointClassification.ORIGIN;
			if (s.End.Equals(p))
				return PointClassification.DESTINATION;
			return PointClassification.BETWEEN;
		}
	}
}