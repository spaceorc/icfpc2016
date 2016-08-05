using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public class FieldAccessor : IAccessor
    {
        FieldInfo info;

        public FieldAccessor(FieldInfo info)
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
                return info.FieldType;
            }
        }

        public object GetValue(object data)
        {
            return info.GetValue(data);
        }
    }

}
