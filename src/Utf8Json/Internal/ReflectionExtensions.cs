using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Utf8Json.Internal
{
    internal static class ReflectionExtensions
    {
        private class OrderBaseTypesBeforeDerivedTypes : IComparer<Type>
        {
            public static readonly OrderBaseTypesBeforeDerivedTypes Instance = new OrderBaseTypesBeforeDerivedTypes();

            private OrderBaseTypesBeforeDerivedTypes()
            {
            }

            public int Compare(Type x, Type y)
            {
                return
                    x.IsEquivalentTo(y) ? 0 :
                    x.IsAssignableFrom(y) ? -1 :
                    y.IsAssignableFrom(x) ? 1 :
                    0;
            }
        }

        public static bool IsNullable(this System.Reflection.TypeInfo type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }

        public static bool IsPublic(this System.Reflection.TypeInfo type)
        {
            return type.IsPublic;
        }

        public static bool IsAnonymous(this System.Reflection.TypeInfo type)
        {
            return type.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            return GetAllPropertiesCore(type, new HashSet<string>())
                .OrderBy(m => m.DeclaringType, OrderBaseTypesBeforeDerivedTypes.Instance);
        }

        static IEnumerable<PropertyInfo> GetAllPropertiesCore(Type type, HashSet<string> nameCheck)
        {
            foreach (var item in type.GetRuntimeProperties())
            {
                if (nameCheck.Add(item.Name))
                {
                    yield return item;
                }
            }
            if (type.BaseType != null)
            {
                foreach (var item in GetAllPropertiesCore(type.BaseType, nameCheck))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<FieldInfo> GetAllFields(this Type type)
        {
            return GetAllFieldsCore(type, new HashSet<string>());
        }

        static IEnumerable<FieldInfo> GetAllFieldsCore(Type type, HashSet<string> nameCheck)
        {
            foreach (var item in type.GetRuntimeFields())
            {
                if (nameCheck.Add(item.Name))
                {
                    yield return item;
                }
            }
            if (type.BaseType != null)
            {
                foreach (var item in GetAllFieldsCore(type.BaseType, nameCheck))
                {
                    yield return item;
                }
            }
        }

#if NETSTANDARD

        public static bool IsConstructedGenericType(this System.Reflection.TypeInfo type)
        {
            return type.AsType().IsConstructedGenericType;
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propInfo)
        {
            return propInfo.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propInfo)
        {
            return propInfo.SetMethod;
        }

#endif
    }
}