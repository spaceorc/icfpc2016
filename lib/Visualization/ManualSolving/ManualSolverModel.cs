using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace lib.Visualization.ManualSolving
{
	public class ManualSolverModel
	{
		public ManualSolverModel(ProblemSpec problem, Vector shift, ImmutableArray<Segment> segments, int? highlightedSegmentIndex, ImmutableList<int> selectedSegmentIndices, PendingOperationType pendingOperation)
		{
			Problem = problem;
			Shift = shift;
			Segments = segments;
			HighlightedSegmentIndex = highlightedSegmentIndex;
			SelectedSegmentIndices = selectedSegmentIndices;
			PendingOperation = pendingOperation;
		}

		public readonly ProblemSpec Problem;
		public readonly Vector Shift;
		public readonly ImmutableArray<Segment> Segments;
		public int? HighlightedSegmentIndex;
		public readonly ImmutableList<int> SelectedSegmentIndices;
		public readonly PendingOperationType PendingOperation;

		public ManualSolverModel(ProblemSpec problem)
		{
			Problem = problem;
			Segments = problem.Segments.ToImmutableArray();
			SelectedSegmentIndices = ImmutableList<int>.Empty;
			Shift = -problem.MinXY();
		}
		public void UpdateHighlightedSegment(Vector p)
		{
			var segment = Segments.OrderBy(s => s.Distance2To(p)).FirstOrDefault();
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
			var mirror = Segments[index];
			var selectedSegments = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			var reflected = selectedSegments.Select(s => s.Reflect(mirror));
			IEnumerable<Segment> res = Segments;
			if (PendingOperation == PendingOperationType.ReflectMove)
				res = res.Where(s => !selectedSegments.Contains(s));
			res = res.Concat(reflected);
			return With(res, null, ImmutableList<int>.Empty);
		}

		private ManualSolverModel With(IEnumerable<Segment> segments, int? highlightedSegmentIndex, ImmutableList<int> selectedSegmentIndices, PendingOperationType pendingOperation = PendingOperationType.None)
		{
			return new ManualSolverModel(Problem, Shift, segments.ToImmutableArray(), highlightedSegmentIndex, selectedSegmentIndices, pendingOperation);
		}

		private ManualSolverModel ToggleHighlighted(int index)
		{
			if (SelectedSegmentIndices.Contains(index))
				return With(Segments, HighlightedSegmentIndex, SelectedSegmentIndices.Remove(index));
			else
				return With(Segments, HighlightedSegmentIndex, SelectedSegmentIndices.Add(index));
		}

		public ManualSolverModel StartOperation(PendingOperationType operation)
		{
			return With(Segments, HighlightedSegmentIndex, SelectedSegmentIndices, operation);
		}

		public ManualSolverModel CancelPendingOperation()
		{
			return With(Segments, HighlightedSegmentIndex, SelectedSegmentIndices, PendingOperationType.None);
		}
	}
}