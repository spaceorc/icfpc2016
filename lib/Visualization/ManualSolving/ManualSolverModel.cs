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
			var segment = Segments.OrderBy(s => (s.Start - p).Length).FirstOrDefault();
			var index = Array.IndexOf(Segments, segment);
			HighlightedSegmentIndex = index < 0 ? (int?) null : index;
		}

		public void ToggleHighlightedToSelected()
		{
			if (!HighlightedSegmentIndex.HasValue) return;
			var index = HighlightedSegmentIndex.Value;
			if (SelectedSegmentIndices.Contains(index))
				SelectedSegmentIndices.Remove(index);
			else
				SelectedSegmentIndices.Add(index);
		}
	}
}