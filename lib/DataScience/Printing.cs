using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public static class PrintingExtensions
    {

        static string Print(List<object> data, Dictionary<string, Func<object, string>> selectors)
        {
            var table = new string[data.Count][];
            var properties = selectors.Keys.ToArray();
            for (int i = 0; i < data.Count; i++)
            {
                table[i] = new string[properties.Length];
                for (int j = 0; j < properties.Length; j++)
                {
                    table[i][j] = selectors[properties[j]](data[i]);
                }
            }
            var lengths = Enumerable
                .Range(0, properties.Length)
                .Select(
                    column => new
                    {
                        column,
                        max = Enumerable.Range(0, data.Count)
                        .Select(i => table[i][column].Length)
                        .Max()
                    })
                .Select(colData => new { column = colData.column, max = Math.Max(properties[colData.column].Length, colData.max) })
                .ToList();

            var formats = lengths
                .Select(coldata => "{" + coldata.column + ",-" + (coldata.max + 2) + "}")
                .Aggregate((a, b) => a + b);

            var builder = new StringBuilder();
            builder.AppendLine(string.Format(formats, properties));
            builder.AppendLine(string.Format(formats, lengths.Select(z => new string('-', z.max)).ToArray()));
            foreach (var e in table)
                builder.AppendLine(string.Format(formats, e));
            return builder.ToString();
        }

        static string PrintObjectEnumerable(List<object> en, Type t)
        {
            var access = new Dictionary<string, Func<object, string>>();

            access = t.GetHierarchicalAccessors()
                .ToDictionary(z => z.Name, z => z.StringValueSelector("NULL"));

       
            return Print(en, access);
        }

        static string PrintDictionaryEnumerable(List<object> en)
        {
            var access = new Dictionary<string, Func<object, string>>();

            foreach (IDictionary dict in en)
                foreach (var key in dict.Keys)
                {
                    var keyStr = key.ToString();
                    if (access.ContainsKey(keyStr)) continue;
                    access[key.ToString()] = new Func<object, string>(z =>
                            {
                                var d = z as IDictionary;
                                if (!d.Contains(key)) return "NULL";
                                return d[key]?.ToString() ?? "NULL";
                            });
                }

            return Print(en, access);
        }
   

        public static void Print(this object data, string caption=null)
        {
            if (caption != null)
                DataConsole.WriteLine(caption);

            var type = data.GetType();
            if (type.IsPrintable())
            {
                DataConsole.WriteLine(data);
                return;
            }

            if (data is IEnumerable)
            {
                var en = (IEnumerable)data;
                var values = new List<object>();
                foreach (var e in en) values.Add(e);

                if (values.Count == 0)
                {
                    DataConsole.WriteLine("EMPTY");
                    return;
                }

                var dtype = values[0].GetType();
                if (dtype.IsPrintable())
                {
                    foreach (var e in values)
                        DataConsole.WriteLine(e);
                    return;
                }

                if (values[0] is IDictionary)
                {
                    DataConsole.WriteLine(PrintDictionaryEnumerable(values));
                    return;
                }

                DataConsole.WriteLine(PrintObjectEnumerable(values, values[0].GetType()));
                return;
            }
            DataConsole.WriteLine(type.Name + " is not printable");
        }
    }
}
