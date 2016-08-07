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
		public ImmutableStack<Segment> mirrors = ImmutableStack<Segment>.Empty;
		public ManualSolverModel(ProblemSpec problem, Vector shift, ImmutableArray<SegmentModel> segments, int? highlightedSegmentIndex, ImmutableList<int> selectedSegmentIndices, PendingOperationType pendingOperation, ImmutableStack<Segment> mirrors)
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
			return With(res, null, ImmutableList<int>.Empty, PendingOperationType.None, mirrors.Push(mirror));
		}

		private ManualSolverModel With(IEnumerable<SegmentModel> segments, int? highlightedSegmentIndex, ImmutableList<int> selectedSegmentIndices, PendingOperationType pendingOperation,
			ImmutableStack<Segment> mirrors)
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

		public SolutionSpec SolveConvex()
		{
			var polygon = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			ProblemSpec problem = CreatProblemSpec(polygon);
			var solutionSpec = ConvexPolygonSolver.TrySolve(problem);
			foreach (var mirror in mirrors.Reverse())
			{
				solutionSpec = solutionSpec.Fold(mirror);
			}
			return solutionSpec;
		}

		private ProblemSpec CreatProblemSpec(List<SegmentModel> polygon)
		{
			var ss = polygon.Select(s => s.Segment).ToArray();
			var ps = ss.Select(s => s.Start).ToArray();
			return new ProblemSpec(new Polygon[] {new Polygon(ps),  }, ss);

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
	}
}