using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;

namespace lib.Visualization.ManualSolving
{
	public class SegmentModel
	{
		public SegmentModel(Segment segment, Color color)
		{
			Segment = segment;
			Color = color;
		}

		public Segment Segment;
		public Segment OriginalSegment;
		public Color Color;

		public SegmentModel Reflect(Segment mirror)
		{
			return new SegmentModel(Segment.Reflect(mirror), Color) {OriginalSegment = OriginalSegment};
		}
	}

	public class ManualSolverModel
	{
		public ImmutableList<Segment> mirrors = ImmutableList<Segment>.Empty;
		public ManualSolverModel(ProblemSpec problem, Vector shift, ImmutableArray<SegmentModel> segments, int? highlightedSegmentIndex, ImmutableList<int> selectedSegmentIndices, PendingOperationType pendingOperation, ImmutableList<Segment> mirrors)
		{
			Problem = problem;
			Shift = shift;
			Segments = segments;
			HighlightedSegmentIndex = highlightedSegmentIndex;
			SelectedSegmentIndices = selectedSegmentIndices;
			PendingOperation = pendingOperation;
			this.mirrors = mirrors;
		}

		public readonly ProblemSpec Problem;
		public readonly Vector Shift;
		public readonly ImmutableArray<SegmentModel> Segments;
		public int? HighlightedSegmentIndex;
		public readonly ImmutableList<int> SelectedSegmentIndices;
		public PendingOperationType PendingOperation;

		public ManualSolverModel(ProblemSpec problem)
		{
			Problem = problem;
			Segments = problem.Segments.Select(s => new SegmentModel(s, GetUsualColor(s))).ToImmutableArray();
			SelectedSegmentIndices = ImmutableList<int>.Empty;
			Shift = -problem.MinXY();
		}

		private static Color GetUsualColor(Segment s)
		{
			return Arithmetic.IsSquare(s.QuadratOfLength) ? Color.Cyan : Color.Black;
		}

		public void UpdateHighlightedSegment(Vector p)
		{
			var segment = Segments.OrderBy(s => s.Segment.Distance2To(p)).FirstOrDefault();
			var index = Segments.IndexOf(segment);
			HighlightedSegmentIndex = index < 0 ? (int?) null : index;
		}

		public ManualSolverModel SelectSegment()
		{
			if (!HighlightedSegmentIndex.HasValue) return this;
			var index = HighlightedSegmentIndex.Value;
			if (PendingOperation == PendingOperationType.None)
				return ToggleHighlighted(index);
			else
			{
				return CompleteOperation(index);
			}
		}

		private ManualSolverModel CompleteOperation(int index)
		{
			var mirror = Segments[index].Segment;
			var selectedSegments = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			var reflected = selectedSegments.Select(s => s.Reflect(mirror));
			IEnumerable<SegmentModel> res = Segments;
			if (PendingOperation == PendingOperationType.ReflectMove)
				res = res.Where(s => !selectedSegments.Contains(s));
			res = res.Concat(reflected);
			PendingOperation = PendingOperationType.None;
			return With(res, null, ImmutableList<int>.Empty, PendingOperationType.None, mirrors.Add(mirror));
		}

		private ManualSolverModel With(IEnumerable<SegmentModel> segments, int? highlightedSegmentIndex, ImmutableList<int> selectedSegmentIndices, PendingOperationType pendingOperation,
			ImmutableList<Segment> mirrors)
		{
			return new ManualSolverModel(Problem, Shift, segments.ToImmutableArray(), highlightedSegmentIndex, selectedSegmentIndices, pendingOperation, mirrors);
		}

		private ManualSolverModel ToggleHighlighted(int index)
		{
			if (SelectedSegmentIndices.Contains(index))
				return With(Segments, HighlightedSegmentIndex, SelectedSegmentIndices.Remove(index), PendingOperationType.None, mirrors);
			else
				return With(Segments, HighlightedSegmentIndex, SelectedSegmentIndices.Add(index), PendingOperationType.None, mirrors);
		}

		public ManualSolverModel StartOperation(PendingOperationType operation)
		{
			PendingOperation = operation;
			return this;
		}

		public ManualSolverModel CancelPendingOperation()
		{
			PendingOperation = PendingOperationType.None;
			return this;
		}

		public IEnumerable<SolutionSpec> SolveConvex()
		{
			var polygon = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			ProblemSpec problem = CreatProblemSpec(polygon);
			var solution = TrySolve(problem);
			if (solution == null) return Enumerable.Empty<SolutionSpec>();
			return GetAllMirrorCombinatons(solution, mirrors);
		}

		private static SolutionSpec TrySolve(ProblemSpec problem)
		{
			if (problem.Polygons.Length > 1 || !problem.Polygons.Single().IsConvex() || problem.Polygons.Single().GetSignedSquare() < 0)
				return null;

			var problemPolygon = problem.Polygons[0];
			var initialSolution = ConvexPolygonSolver.TryGetInitialSolution(problem, problemPolygon);
			if (initialSolution == null)
				return null;

			return ConvexPolygonSolver.TrySolve(problemPolygon, initialSolution);
		}

		private IEnumerable<SolutionSpec> GetAllMirrorCombinatons(SolutionSpec solution, ImmutableList<Segment> mirrors)
		{
			if (mirrors.IsEmpty) yield return solution;
			else
			{
				var mirror = mirrors[mirrors.Count - 1];
				var leftMirrors = mirrors.RemoveAt(mirrors.Count - 1);
				foreach (var s in GetAllMirrorCombinatons(solution.Fold(mirror), leftMirrors))
					yield return s;
				foreach (var s in GetAllMirrorCombinatons(solution.Fold(new Segment(mirror.End, mirror.Start)), leftMirrors))
					yield return s;
			}
		}

		private ProblemSpec CreatProblemSpec(List<SegmentModel> polygon)
		{
			var ss = polygon.Select(s => s.Segment).ToArray();
			var ps = ss.SelectMany(s => new[] { s.Start, s.End }).Distinct().ToArray();
			var convexHull = new Polygon(ps).GetConvexBoundary();
			var convexProblem = new ProblemSpec(new Polygon[] {convexHull,  }, ss);
			convexProblem.CreateVisualizerForm().Show();
			return convexProblem;

		}

		public ManualSolverModel MarkAsBorder()
		{
			var selectedSegments = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			var border = selectedSegments.Select(s => new SegmentModel(s.Segment, Color.BlueViolet));
			var res = Segments.Where(s => !selectedSegments.Contains(s));
			res = res.Concat(border);
			return With(res, null, ImmutableList<int>.Empty, PendingOperationType.None, mirrors);
		}

		public ManualSolverModel MarkAsNoBorder()
		{
			var selectedSegments = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			var border = selectedSegments.Select(s => new SegmentModel(s.Segment, GetUsualColor(s.Segment)));
			var res = Segments.Where(s => !selectedSegments.Contains(s));
			res = res.Concat(border);
			return With(res, null, ImmutableList<int>.Empty, PendingOperationType.None, mirrors);
		}

		public ManualSolverModel SelectAll()
		{
			return With(Segments, HighlightedSegmentIndex, Enumerable.Range(0, Segments.Length).ToImmutableList(), PendingOperationType.None, mirrors);
		}
	}
}