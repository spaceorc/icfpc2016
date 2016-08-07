using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bingo.Utils;
using lib;

namespace SquareConstructor
{
	public class ConstructorSolver
	{
		private Dictionary<Segment, List<Polygon>> Segments = new Dictionary<Segment, List<Polygon>>();
		private HashSet<Polygon> UsedPolygons = new HashSet<Polygon>();
		private Dictionary<Vector, List<Polygon>> Vertexes = new Dictionary<Vector, List<Polygon>>();
		private HashSet<Vector> UsedVertexes = new HashSet<Vector>();
		private Polygon[] GivenPolygons; 
		private SegmentsMatrix SegmentsMatrix = new SegmentsMatrix(10);
		private Dictionary<Segment, List<Polygon>> GivenGraph;

		private Dictionary<Vector, List<List<Polygon>>> Variants = new Dictionary<Vector, List<List<Polygon>>>();

		private ProblemSpec spec;

		public ConstructorSolver(ProblemSpec spec)
		{
			this.spec = spec;
			int i = 0;
			GivenPolygons = PolygonFinder.GetRealPolygons(spec).Select(p => { p.Id = i++; return p; }).ToArray(); 
			i = 0;
			GivenGraph =
				GivenPolygons.SelectMany(polygon => polygon.Segments.Select(segment => new { polygon, segment }))
					.GroupBy(pair => pair.segment)
					.Select(group => { group.Key.Id = i++; group.ForEach(pair => pair.segment.Id = group.Key.Id); return group; })
					.ToDictionary(pair => pair.Key, pair => pair.Select(s => s.polygon).ToList());
		}

		public SolutionSpec Work()
		{
			if (GivenPolygons.Length == 0)
			{
				GivenPolygons = spec.Polygons.ToArray();
			}

			GivenPolygons
				.OrderByDescending(p => p.GetUnsignedSquare())
				.First(polygon =>
				{
					return polygon.Segments.Any(segment => StartWithPolygonSegment(polygon, segment));
				}
			);
			
			return GenerateSolution();
		}

		private SolutionSpec GenerateSolution()
		{
			var dict = new Dictionary<Vector, int>();
			var points = UsedPolygons.SelectMany(p => p.Vertices).Distinct().ToArray();
			for (int i = 0; i < points.Length; i++)
			{
				dict[points[i]] = i;
			}
			var facets = UsedPolygons.Select(polygon => new Facet(polygon.Vertices.Select(v => dict[v]).ToArray())).ToArray();
			var dest = points.Select(GetDestVector).ToArray();

			return new SolutionSpec(points, facets, dest);
		}

		private Vector GetDestVector(Vector vector)
		{
			var poligon = Vertexes[vector][0];
			int vertId = 0;
			for (int i = 0; i < poligon.Vertices.Length; i++)
			{
				if (vector.Equals(poligon.Vertices[i]))
				{
					vertId = i;
					break;
				}
			}
			return GivenPolygons[poligon.Id].Vertices[vertId];
		}

		private bool StartWithPolygonSegment(Polygon polygon, Segment segment)
		{
			if (!Arithmetic.IsSquare(segment.QuadratOfLength))
				return false;

			var destSegment = new Segment(new Vector(0, 0), new Vector(Arithmetic.Sqrt(segment.QuadratOfLength), 0));
			var transpOperator = TransposeOperator.ConstructOperator(segment, destSegment);

			var fixedPolygon = transpOperator.TransposePolygon(polygon);
			if (SegmentsMatrix.TryAddPolygon(fixedPolygon))
			{
				SetPolygon(fixedPolygon);
				var success = DoIt();
				if(!success)
					RemovePolygon(fixedPolygon);
				return success;
			}
			fixedPolygon = fixedPolygon.Reflect(destSegment);
			if (SegmentsMatrix.TryAddPolygon(fixedPolygon))
			{
				SetPolygon(fixedPolygon);
				var success = DoIt();
				if (!success)
					RemovePolygon(fixedPolygon);
				return success;
			}
			return false;
		}

		private bool SetPolygon(Polygon polygon)
		{
			if(!SegmentsMatrix.TryAddPolygon(polygon))
				return false;
			polygon.Segments.ForEach(segment => Segments.AddToList(segment, polygon));
			polygon.Vertices.ForEach(v => Vertexes.AddToList(v, polygon));
			polygon.Vertices.ForEach(v => Variants.Remove(v));
			if (UsedPolygons.Add(polygon))
				Square = Square + polygon.GetUnsignedSquare();
			return true;
		}

		private void RemovePolygon(Polygon polygon)
		{
			polygon.Segments.ForEach(segment => Segments.RemoveFromList(segment, polygon));
			polygon.Vertices.ForEach(v => Vertexes.RemoveFromList(v, polygon));
			polygon.Vertices.ForEach(v => UsedVertexes.Remove(v));
			polygon.Vertices.ForEach(v => Variants.Remove(v));
			SegmentsMatrix.RemovePolygon(polygon);
			if (UsedPolygons.Remove(polygon))
				Square -= polygon.GetUnsignedSquare();
		}
		
		private bool DoIt()
		{
			if (Square == 1)
				return GivenPolygons.All(g => UsedPolygons.Any(p => p.Id == g.Id));

			foreach (var pair in Vertexes.Where(pair => !Variants.ContainsKey(pair.Key) && !UsedVertexes.Contains(pair.Key)))
			{
				var rounds = GetRounds(pair.Key, 1000); //возвращает Null если цикл уже построен
				if (rounds != null)
					Variants[pair.Key] = rounds;
				else
				{
					UsedVertexes.Add(pair.Key);
					continue;
				}
				if (Variants[pair.Key].Count == 0)
					return false;
			}

			var simplest = Variants.Min(v => v.Value.Count);
			var variant = Variants.First(v => v.Value.Count == simplest);

			bool success = false;
			UsedVertexes.Add(variant.Key);
			foreach (var polygons in variant.Value)
			{
				if (!polygons.All(SetPolygon))
				{
					polygons.ForEach(RemovePolygon);
					continue;
				}

				success = DoIt();
				if (success)
					break;
				polygons.ForEach(RemovePolygon);
			}
			UsedVertexes.Remove(variant.Key);

			return success;
		}

		private Rational Square = new Rational(0, 1);
		
		private List<List<Polygon>> GetRounds(Vector vertex, int maxCount)
		{
			var startSegment =
				Vertexes[vertex].SelectMany(polygon => polygon.Segments.Select(segment => new {segment, polygon}))
					.Where(pair => IsEndOfSegment(vertex, pair.segment))
					.Where(pair => !IsOnSquareBound(pair.segment))
					.Select(pair => Tuple.Create(pair.polygon, pair.segment))
					.FirstOrDefault(pair => (Segments.GetOrDefault(pair.Item2)?.Count ?? 0) < 2);

			if (startSegment == null)
				return null;

			List<List<Polygon>> variants = new List<List<Polygon>>();
			UsedPolygonsInStack.Add(startSegment.Item1);
			DoRound(startSegment.Item1, startSegment.Item2, startSegment.Item2, variants, new Stack<Polygon>(), vertex,
				maxCount, 0);
			UsedPolygonsInStack.Remove(startSegment.Item1);


			//var count = Vertexes.GetOrDefault(vertex)?.Count ?? 0;
			//if (!(vertex.X == 0 || vertex.Y == 0 || vertex.X == 1 || vertex.Y == 1))
			//	variants = variants.Where(vars => (vars.Count + count)%2 == 0).ToList();
			


			return variants;
		}

		private static bool IsEndOfSegment(Vector vertex, Segment segment)
		{
			return segment.Start.Equals(vertex) || segment.End.Equals(vertex);
		}

		private HashSet<Polygon> UsedPolygonsInStack = new HashSet<Polygon>(); 

		private void DoRound(Polygon polygon, Segment startSegment, Segment segment, List<List<Polygon>> result,
			Stack<Polygon> stack, Vector vertex, int maxCount, int deep)
		{
			if(result.Count > maxCount)
				return;

			if(deep > 25)
				Console.WriteLine("");

			if (startSegment.Equals(segment) && stack.Count > 0 || IsOnSquareBound(segment))
			{
				result.Add(stack.ToList());
				return;
			}

			if (Segments.ContainsKey(segment))
			{
				var skipPolygons = Segments[segment].Where(p => !UsedPolygonsInStack.Contains(p)).ToList();
				if (skipPolygons.Count > 0)
				{
					var skipSegment = skipPolygons[0].Segments.First(s => IsEndOfSegment(vertex, s) && !s.Equals(segment));
					UsedPolygonsInStack.Add(skipPolygons[0]);
					DoRound(skipPolygons[0], startSegment, skipSegment, result, stack, vertex, maxCount, deep + 1);
					UsedPolygonsInStack.Remove(skipPolygons[0]);
					return;
				}
			}

			var originalSegment = GivenPolygons[polygon.Id].Segments.First(s => segment.Id == s.Id);
			var trOperator = TransposeOperator.ConstructOperator(originalSegment, segment);
			var possiblePolygons = GivenGraph[originalSegment]
				.Select(p => (p.Id == polygon.Id) ^ polygon.IsReflected ? p.Reflect(originalSegment) : p)
				.Select(p => trOperator.TransposePolygon(p));

			foreach (var pPolygon in possiblePolygons)
			{
				if(!SegmentsMatrix.TryAddPolygon(pPolygon))
					continue;
				stack.Push(pPolygon);
				DoRound(pPolygon, startSegment, pPolygon.Segments.Where(s => s.Start.Equals(vertex) || s.End.Equals(vertex)).First(s => !s.Equals(segment)), result, stack, vertex, maxCount, deep + 1);
				stack.Pop();
				SegmentsMatrix.RemovePolygon(pPolygon);
			}
		}

		private bool IsOnSquareBound(Segment segment)
		{
			return (segment.Start.X == segment.End.X && (segment.Start.X == 0 || segment.Start.X == 1) ||
				   segment.Start.Y == segment.End.Y && (segment.Start.Y == 0 || segment.Start.Y == 1)) && Arithmetic.IsSquare(segment.QuadratOfLength);
		}
	}
}
