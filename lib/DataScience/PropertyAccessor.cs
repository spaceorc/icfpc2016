using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public class PropertyAccessor : IAccessor
    {
        PropertyInfo info;

        public PropertyAccessor(PropertyInfo info)
        {
            this.info = info;
        }

        public string Name
        {
            get
            {
                return info.Name;
            }
        }

        public Type Type
        {
            get
            {
                return info.PropertyType;
            }
        }

        public object GetValue(object data)
        {
            return info.GetValue(data);
        }
    }

}
