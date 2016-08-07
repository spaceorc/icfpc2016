using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Bingo.Utils
{
	public static class DictionaryExtension
	{
		public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
		{
			if(dict == null)
				return defaultValue;
			if(key == null)
				return defaultValue;
			TValue value;
			return dict.TryGetValue(key, out value) ? value : defaultValue;
		}

		public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
		{
			if(dict == null)
				return defaultValue;
			if(key == null)
				return defaultValue;
			TValue value;
			return dict.TryGetValue(key, out value) ? value : defaultValue;
		}

		public static string GetOrSelf(this Dictionary<string, string> dict, string val)
		{
			if(dict == null || val == null)
				return val;
			string mapped;
			return dict.TryGetValue(val, out mapped) ? mapped : val;
		}

		public static void AddToList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue addValue)
		{
			List<TValue> list;
			if (!dict.TryGetValue(key, out list))
				dict[key] = new List<TValue> { addValue };
			else
				list.Add(addValue);
		}

		public static void RemoveFromList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue removeValue)
		{
			List<TValue> list;
			if(!dict.TryGetValue(key, out list))
				return;

			list.Remove(removeValue);
			if (list.Count == 0)
				dict.Remove(key);
		}

		public static void AddToList<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> dict, TKey key, TValue addValue)
		{
			dict.AddOrUpdate(key, inn => new List<TValue> { addValue }, (inn, list) =>
			{
				lock (list)
				{
					list.Add(addValue);
					return list;
				}
			});
		}

		public static bool AddToSet<TKey, TValue>(this ConcurrentDictionary<TKey, HashSet<TValue>> dict, TKey key, TValue addValue)
		{
			var isNew = true;
			dict.AddOrUpdate(key, inn => new HashSet<TValue> { addValue }, (inn, list) =>
			{
				lock (list)
				{
					isNew = list.Add(addValue);
					return list;
				}
			});
			return isNew;
		}

		public static List<TValue> GetClonedSet<TKey, TValue>(this ConcurrentDictionary<TKey, HashSet<TValue>> dict, TKey key)
		{
			var set = dict.GetOrDefault(key);
			lock(set)
			{
				return set.ToList();
			}
		}


		public static bool RemoveFromSet<TKey, TValue>(this ConcurrentDictionary<TKey, HashSet<TValue>> dict, TKey key, TValue removeValue)
		{
			var success = true;
			dict.AddOrUpdate(key, inn => new HashSet<TValue>(), (inn, list) =>
			{
				lock (list)
				{
					success = list.Remove(removeValue);
					return list;
				}
			});
			return success;
		}

		public static void AddToUniqList<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> dict, TKey key, TValue addValue)
		{
			dict.AddOrUpdate(key, inn => new List<TValue> { addValue }, (inn, list) =>
			{
				lock (list)
				{
					if (!list.Contains(addValue))
						list.Add(addValue);
					return list;
				}
			});
		}

		public static bool IsInList<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
		{
			return dict.GetOrDefault(key)?.Contains(value) ?? false;
		}

		public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
		{
			TValue temp;
			return dict.TryRemove(key, out temp);
		}
	}
}