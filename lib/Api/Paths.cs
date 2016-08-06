using System;
using System.IO;
using NUnit.Framework;

namespace lib
{
	public static class Paths
	{
		public static string ProblemsDir() => 
			Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../problems");
	}
}