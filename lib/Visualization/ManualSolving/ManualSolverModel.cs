using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Visualization.ManualSolving
{
	public class ManualSolverModel
	{
		public ProblemSpec Problem;
		public Vector Shift;
		public Segment[] Segments;
		public int? HighlightedSegmentIndex;
		public List<int> SelectedSegmentIndices;
		public PendingOperationType PendingOperation;

		public ManualSolverModel(ProblemSpec problem)
		{
			Problem = problem;
			Segments = problem.Segments.ToArray();
			SelectedSegmentIndices = new List<int>();
			Shift = problem.MinXY();
		}

		public void UpdateHighlightedSegment(Vector p)
		{
			var segment = Segments.OrderBy(s => s.Distance2To(p)).FirstOrDefault();
			var index = Array.IndexOf(Segments, segment);
			HighlightedSegmentIndex = index < 0 ? (int?) null : index;
		}

		public void SelectSegment()
		{
			if (!HighlightedSegmentIndex.HasValue) return;
			var index = HighlightedSegmentIndex.Value;
			if (PendingOperation == PendingOperationType.None)
				ToggleHighlighted(index);
			else
			{
				CompleteOperation(index);
			}
		}

		private void CompleteOperation(int index)
		{
			var mirror = Segments[index];
			var selectedSegments = SelectedSegmentIndices.Select(i => Segments[i]).ToList();
			var reflected = selectedSegments.Select(s => s.Reflect(mirror)).ToArray();
			if (PendingOperation == PendingOperationType.ReflectMove)
				Segments = Segments.Where(s => !selectedSegments.Contains(s)).ToArray();
			Segments = Segments.Concat(reflected).ToArray();
			PendingOperation = PendingOperationType.None;
			SelectedSegmentIndices.Clear();
		}

		private void ToggleHighlighted(int index)
		{
			if (SelectedSegmentIndices.Contains(index))
				SelectedSegmentIndices.Remove(index);
			else
				SelectedSegmentIndices.Add(index);
		}

		public void StartOperation(PendingOperationType operation)
		{
			PendingOperation = operation;
		}
		public void CancelPendingOperation()
		{
			PendingOperation = PendingOperationType.None;
		}
	}
}