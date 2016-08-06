using NUnit.Framework;

namespace lib.Visualization.ManualSolving
{
	[TestFixture]
	public class ManualSolverForm_Should
	{
		[Test]
		public void Open()
		{
			new ManualSolverForm(new ProblemsRepo().Get(9)).ShowDialog();
		}
	}
}