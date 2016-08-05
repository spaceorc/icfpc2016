using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class IEnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> en)
        {
            var set = new HashSet<T>();
            foreach (var e in en)
                set.Add(e);
            return set;
        }

        public static void ForEach<T>(this IEnumerable<T> en, Action<T> action)
        {
            foreach (var e in en)
                action(e);
        }

        public class CountByResult<TKey>
        {
            public TKey Key;
            public int Count;
        }

        public static IQueryable<CountByResult<TKey>> CountBy<TData, TKey>(this IQueryable<TData> data, Expression<Func<TData, TKey>> key)
        {
            return data.GroupBy(key).Select(z => new CountByResult<TKey> { Key = z.Key, Count = z.Count() }).OrderByDescending(z => z.Count);
        }

        public static IEnumerable<CountByResult<TKey>> CountBy<TData, TKey>(this IEnumerable<TData> data, Func<TData, TKey> key)
        {
            return data.GroupBy(key).Select(z => new CountByResult<TKey> { Key = z.Key, Count = z.Count() }).OrderByDescending(z => z.Count);
        }

        public static TValue SafeGet<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key)
            where TValue : new()
        {
            if (!d.ContainsKey(key)) d[key] = new TValue();
            return d[key];
        }

        public static TValue SafeGet<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, Func<TValue> createValue)
        {
            if (!d.ContainsKey(key)) d[key] = createValue();
            return d[key];
        }

        public static void SafeReplace<TKey,TValue>(this Dictionary<TKey, TValue> d, TKey key, Func<TValue,TValue> replacer)
            where TValue : new()
        {
            if (!d.ContainsKey(key)) d[key] = new TValue();
            d[key] = replacer(d[key]);
        }



        public static IEnumerable<TResult> CollectDataWithState<TData, TKey, TState, TResult>(
            this IEnumerable<TData> data,
            Func<TData, TKey> keySelector,
            Func<TState> newState,
            Func<TData, TState, TResult> process
            )
        {
            var dictionary = new Dictionary<TKey, TState>();
            foreach (var e in data)
            {
                var key = keySelector(e);
                if (!dictionary.ContainsKey(key)) dictionary[key] = newState();
                var state = dictionary[key];
                var result = process(e, state);
                if (result != null) yield return result;
            }
        }

        public static IEnumerable<Tuple<T, T>> SelfZip<T>(this IEnumerable<T> en)
        {
            var prev = default(T);
            bool firstTime = true;
            foreach (var e in en)
            {
                if (!firstTime) yield return Tuple.Create(prev, e);
                firstTime = false;
                prev = e;
            }
        }

        public static IEnumerable<T> ShowProgress<T>(this IEnumerable<T> en, int rate)
        {
            int t = 0;
            foreach (var e in en)
            {
                if (t % rate == 0) Console.Write($"\r{t}                    ");
                t++;
                yield return e;
            }
            Console.WriteLine();
        }

        public static double[] Averages<T>(this IEnumerable<IEnumerable<T>> data, Func<T, double> selector)
        {
            var result = new List<double>();
            int count = 0;
            foreach (var e in data)
            {
                count++;
                int ptr = 0;
                foreach (var z in e)
                {
                    while (result.Count <= ptr) result.Add(0);
                    result[ptr++] += selector(z);
                }
            }
            return result.Select(z => z / count).ToArray();
        }

        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }
        }

        public static IEnumerable<TData> LocalSort<TData>(this IEnumerable<TData> data, Func<TData, DateTime> selector, TimeSpan overfillDetector)
        {
            return data.LocalSort(selector, (end , start)=>end - start > overfillDetector);
        }



        public static IEnumerable<TData> LocalSort<TData, TKey>(this IEnumerable<TData> data, Func<TData, TKey> selector, Func<TKey, TKey, bool> overfillDetector)
            where TKey : IComparable
        {
            var list = new SortedList<TKey, TData>(new DuplicateKeyComparer<TKey>());
            TKey lastSentKey = default(TKey);
            bool dataWereSent = false;
            foreach (var e in data)
            {
                var key = selector(e);
                if (dataWereSent && lastSentKey.CompareTo(key) > 0)
                {
                    var first = list.First();
                    var last = list.Last();
                    throw new Exception();
                }
                    
                list.Add(key, e);
                while (true)
                {
                    if (list.Count < 2) break;
                    var deletionKey = list.Keys[0];
                    if (!overfillDetector(list.Keys[list.Keys.Count - 1], deletionKey)) break;
                    yield return list.Values[0];
                    list.RemoveAt(0);
                    lastSentKey = deletionKey;
                    dataWereSent = true;
                }
            }
            foreach (var e in list)
                yield return e.Value;
        }

        public static IEnumerable<TData> CheckIfSorted<TData, TKey>(this IEnumerable<TData> data, Func<TData, TKey> selector)
            where TKey : IComparable
        {
            var lastKey = default(TKey);
            var firstTime = true;
            foreach (var e in data)
            {
                var thisKey = selector(e);
                if (!firstTime && lastKey.CompareTo(thisKey) > 0) throw new Exception();
                lastKey = thisKey;
                firstTime = false;
                yield return e;
            }
        }
        

        public class Period<T>
        {
            public DateTime StartTime;
            public DateTime EndTime;
            public List<T> Data = new List<T>();
        }


        public static IEnumerable<Period<T>> Periodize<T>(this IEnumerable<T> en, Func<T, DateTime> timeSelector, DateTime begin, TimeSpan periodLength, DateTime end)
        {
            var period = new Period<T> { StartTime = begin, EndTime = begin + periodLength };
            foreach (var e in en.CheckIfSorted(z => timeSelector(z)))
            {
                var time = timeSelector(e);
                while (time > period.EndTime)
                {
                    yield return period;
                    period = new Period<T> { StartTime = period.EndTime, EndTime = period.EndTime + periodLength };
                }
                period.Data.Add(e);
            }
            yield return period;
            while(period.EndTime<end)
            {
                period = new Period<T> { StartTime = period.EndTime, EndTime = period.EndTime + periodLength };
                yield return period;
            }
        }

        public class ItemWithIndex<T>
        {
            public T Item;
            public int Index;
        }


        public static IEnumerable<ItemWithIndex<T>> WithIndices<T>(this IEnumerable<T> en)
        {
            int index = 0;
            foreach (var e in en)
                yield return new ItemWithIndex<T> { Item = e, Index = index++ };
        }

        

        public static IEnumerable<double> Normalize(this IEnumerable<double> data)
        {
            var list = data.ToList();
            var max = list.Max();
            var min = list.Min();
            return list.Select(z => (z-min) / (max-min));
        }

        public static IEnumerable<double> NormalizeByMax(this IEnumerable<double> data)
        {
            var list = data.ToList();
            var max = list.Max();
            return list.Select(z => z/max);
        }

        public static IEnumerable<double> Normalize2(this IEnumerable<double> data)
        {
            var list = data.ToList();
            var max = list.Max();
            var min = list.Min();
            return list.Select(z => -1+2*(z - min) / (max - min));
        }

        public static IEnumerable<int> Which<T>(this IEnumerable<T> data, Func<T, bool> predicate)
        {
            int count = 0;
            foreach (var e in data)
            {
                if (predicate(e)) yield return count;
                count++;
            }
        }
        public static int ArgMax<T>(this IEnumerable<T> data, Func<T,double> selector)
        {
            int result = 0;
            double value = double.NegativeInfinity;
            int counter = 0;
            foreach(var e in data)
            {
                var newValue = selector(e);
                if (newValue > value)
                {
                    value = newValue;
                    result = counter;
                }
                counter++;
            }
            return result;

        }
    }
}
