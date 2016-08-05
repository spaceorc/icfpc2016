using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public class CombinedAccessor : IAccessor
    {
        List<IAccessor> accessors;
        public CombinedAccessor(List<IAccessor> accessors)
        {
            this.accessors = accessors;
        }

        public string Name
        {
            get
            {
                return accessors.Select(z => z.Name).Aggregate((a, b) => a + "." + b);
            }
        }

        public Type Type
        {
            get
            {
                return accessors.Last().Type;
            }
        }

        public object GetValue(object data)
        {
            foreach(var e in accessors)
            {
                data = e.GetValue(data);
            }
            return data;
        }
    }
}
