using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public interface IAccessor
    {
        Type Type { get; }
        string Name { get; }
        object GetValue(object data);
    }

    public static class AccessorExtensions
    {
        public static Func<object,string> StringValueSelector(this IAccessor accessor, string nullString)
        {
            return z => accessor.GetValue(z)?.ToString() ?? nullString;
        }
    }
    
}
