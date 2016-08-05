using System.IO;
using NUnit.Framework;

namespace lib
{
	public static class Paths
	{
		public static string ProblemsDir() => 
			Path.Combine(new DirectoryInfo(TestContext.CurrentContext.TestDirectory).Parent?.Parent?.Parent?.FullName ?? ".", "problems");

	}
}