using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StatePrinting;
using StatePrinting.FieldHarvesters;

namespace lib
{
	public static class ObjectExtensions
	{
		public static string StrJoin<T>(this IEnumerable<T> items, string delimiter)
		{
			return string.Join(delimiter, items);
		}
		public static string StrJoin<T>(this IEnumerable<T> items, string delimiter, Func<T, string> toString)
		{
			return items.Select(toString).StrJoin(delimiter);
		}
		public static string ToDebugString<T>(this T obj, Action<ProjectionHarvester> config = null)
		{
			var stateprinter = new Stateprinter();
			config?.Invoke(stateprinter.Configuration.Project);
			return stateprinter.PrintObject(obj);
		}

		public static StringReader ToReader<T>(this string s)
		{
			return new StringReader(s);
		}
	}
}