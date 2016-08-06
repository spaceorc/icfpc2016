using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public static class GraphExtensions
    {
        public static IEnumerable<Node<TE,TN>> AllNodes<TE,TN>(this IEnumerable<Edge<TE,TN>> edges)
        {
            foreach(var e in edges)
            {
                yield return e.From;
                yield return e.To;
            }
        }
    }
}
