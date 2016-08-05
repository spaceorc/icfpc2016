using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace lib
{
	public static class ObjectExtensions
	{
		public static string StrJoin<T>(this IEnumerable<T> items, string delimiter)
		{
			return string.Join(delimiter, items);
		}
		public static StringReader ToReader<T>(this string s)
		{
			return new StringReader(s);
		}
	}
}