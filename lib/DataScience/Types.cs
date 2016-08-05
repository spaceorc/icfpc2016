using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public class FieldOrProperty
    {
        PropertyInfo property;
        FieldInfo field;
        MemberInfo member;
        public string Name;
        public Type Type;
        public object GetValue(object data)
        {
            if (property != null) return property.GetValue(data);
            return field.GetValue(data);
        }
        public FieldOrProperty(MemberInfo info)
        {
            member = info;
            property = info as PropertyInfo;
            field = info as FieldInfo;
            if (property == null && field == null) throw new ArgumentException();
            Name = info.Name;
            if (property!=null)
            {
                Type = property.PropertyType;
            }
            if (field!=null)
            {
                Type = field.FieldType;
            }
            
        }
        public Func<object, string> ValueSelector(string nullString)
        {
            return z => GetValue(z)?.ToString() ?? nullString;
        }

        public Expression ValueSelectorExpression<T>(Expression parameter, string nullString, string quotation="\"")
        {
            var ToString = Expression.Call(
                        Expression.MakeMemberAccess(parameter, member),
                        Type.GetMethod("ToString", new Type[0]));

            if (!Type.IsNumericOrNullable())
                ToString =
                    Expression.Call(
                        null,
                        typeof(string).GetMethod("Concat", new[] { typeof(string[]) }),
                        Expression.NewArrayInit(
                            typeof(string),
                            Expression.Constant(quotation),
                            ToString,
                            Expression.Constant(quotation)));

            if (!Types.NullableTypes.Contains(Type))
                return ToString;

            return Expression.Condition(
                    Expression.NotEqual(
                        Expression.MakeMemberAccess(parameter, member),
                        Expression.Constant(null)),
                    ToString,
                    Expression.Constant(nullString));
        }
    }


    public static class Types
    {
        static IEnumerable<Type> NullableVersions(IEnumerable<Type> types)
        {
            return types
                 .Where(z => z.IsValueType)
                 .Select(z => typeof(Nullable<>).MakeGenericType(z));
        }

        public static HashSet<Type> KnownTypes;
        public static HashSet<Type> NullableTypes;
        public static HashSet<Type> NumericTypes;
        public static HashSet<Type> NumericOrNullableNumericTypes;

        public static bool IsNumericOrNullable(this Type t)
        {
            return NumericOrNullableNumericTypes.Contains(t);
        }

        public static bool IsPrintable(this Type t)
        {
            return KnownTypes.Contains(t);
        }

        public static IEnumerable<FieldOrProperty> GetFieldsAndProperties(this Type t)
        {
            return t
                .GetMembers()
                .Where(z => z is PropertyInfo || z is FieldInfo)
                .Select(z => new FieldOrProperty(z))
                .ToList();
        }

        static void GetAccessors(Type t, List<IAccessor> history, List<List<IAccessor>> results)
        {
            var acc = t
                .GetMembers()
                .Where(z => z is PropertyInfo || z is FieldInfo)
                .Select(z => z is PropertyInfo ? (IAccessor)new PropertyAccessor(z as PropertyInfo) : (IAccessor)new FieldAccessor(z as FieldInfo))
                .ToList();
            foreach(var e in acc)
            {
                var newList= history.ToList();
                newList.Add(e);

                if (e.Type.IsPrintable())
                    results.Add(newList);
                else
                    GetAccessors(e.Type, newList, results);
            }
        }

        public static IEnumerable<IAccessor> GetHierarchicalAccessors(this Type t)
        {
            var result = new List<List<IAccessor>>();
            GetAccessors(t, new List<IAccessor>(), result);
            var compact = result.Select(z => z.Count == 1 ? z[0] : new CombinedAccessor(z)).ToList();
            return compact;
        }

        static Types()
        {
            var numeric = new List<Type>
            {
                typeof(byte),
                typeof(sbyte),
                typeof(int),
                typeof(uint),
                typeof(short),
                typeof(ushort),
                typeof(long),
                typeof(ulong),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(BigInteger)
            };

            NumericTypes = numeric.ToHashSet();
            NumericOrNullableNumericTypes = NullableVersions(NumericTypes).Concat(NumericTypes).ToHashSet();

            var printable = new List<Type>
            {
                typeof(bool),
                typeof(string),
                typeof(Guid),
                typeof(DateTime)
            };

            printable.AddRange(numeric);

            NullableTypes = printable
                .Where(z => !z.IsValueType)
                .Concat(NullableVersions(printable))
                .ToHashSet();

            KnownTypes = NullableVersions(printable).Concat(printable).ToHashSet();
        }
    }
}
