using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System
{
    [DebuggerStepThrough]
    internal static class SharedTypeExtensions
    {
        public static Type UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNullableType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsValueType
                   || (typeInfo.IsGenericType
                       && (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }
        public static bool IsValidEntityType(this Type type)
            => type.GetTypeInfo().IsClass;

        public static Type MakeNullable(this Type type)
            => type.IsNullableType()
                ? type
                : typeof(Nullable<>).MakeGenericType(type);

        public static bool IsInteger(this Type type)
        {
            type = type.UnwrapNullableType();

            return (type == typeof(int))
                   || (type == typeof(long))
                   || (type == typeof(short))
                   || (type == typeof(byte))
                   || (type == typeof(uint))
                   || (type == typeof(ulong))
                   || (type == typeof(ushort))
                   || (type == typeof(sbyte))
                   || (type == typeof(char));
        }

        public static PropertyInfo GetAnyProperty(this Type type, string name)
        {
            var props = type.GetRuntimeProperties().Where(p => p.Name == name).ToList();
            if (props.Count > 1)
            {
                throw new AmbiguousMatchException();
            }

            return props.SingleOrDefault();
        }

        private static bool IsNonIntegerPrimitive(this Type type)
        {
            type = type.UnwrapNullableType();

            return (type == typeof(bool))
                   || (type == typeof(byte[]))
                   || (type == typeof(DateTime))
                   || (type == typeof(DateTimeOffset))
                   || (type == typeof(decimal))
                   || (type == typeof(double))
                   || (type == typeof(float))
                   || (type == typeof(Guid))
                   || (type == typeof(string))
                   || (type == typeof(TimeSpan))
                   || type.GetTypeInfo().IsEnum;
        }

        public static bool IsPrimitive(this Type type)
            => type.IsInteger() || type.IsNonIntegerPrimitive();

        public static bool IsInstantiable(this Type type) => IsInstantiable(type.GetTypeInfo());

        private static bool IsInstantiable(TypeInfo type)
            => !type.IsAbstract
               && !type.IsInterface
               && (!type.IsGenericType || !type.IsGenericTypeDefinition);

        public static bool IsGrouping(this Type type) => IsGrouping(type.GetTypeInfo());

        private static bool IsGrouping(TypeInfo type)
            => type.IsGenericType
               && (type.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                   || type.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>));

        public static Type UnwrapEnumType(this Type type)
        {
            var isNullable = type.IsNullableType();
            var underlyingNonNullableType = isNullable ? type.UnwrapNullableType() : type;
            if (!underlyingNonNullableType.GetTypeInfo().IsEnum)
            {
                return type;
            }

            var underlyingEnumType = Enum.GetUnderlyingType(underlyingNonNullableType);
            return isNullable ? MakeNullable(underlyingEnumType) : underlyingEnumType;
        }

        public static Type GetSequenceType(this Type type)
        {
            var sequenceType = TryGetSequenceType(type);
            if (sequenceType == null)
            {
                // TODO: Add exception message
                throw new ArgumentException();
            }

            return sequenceType;
        }

        public static Type TryGetSequenceType(this Type type)
            => type.IsArray
                ? type.GetElementType()
                : type.TryGetElementType(typeof(IDictionary<,>), 1)
                   ?? type.TryGetElementType(typeof(IEnumerable<>))
                   ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType, int elementIndex = 0)
        {
            if (!type.GetTypeInfo().IsGenericTypeDefinition)
            {
                var types = GetGenericTypeImplementations(type, interfaceOrBaseType).ToList();

                return types.Count == 1 ? types[0].GetTypeInfo().GenericTypeArguments[elementIndex] : null;
            }

            return null;
        }

        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                return (interfaceOrBaseType.GetTypeInfo().IsInterface ? typeInfo.ImplementedInterfaces : type.GetBaseTypes())
                    .Union(new[] { type })
                    .Where(
                        t => t.GetTypeInfo().IsGenericType
                             && (t.GetGenericTypeDefinition() == interfaceOrBaseType));
            }

            return Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> GetImplementationTypes(this Type type, Type interfaceOrBaseType)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            return (interfaceOrBaseType.GetTypeInfo().IsInterface ? typeInfo.ImplementedInterfaces : type.GetBaseTypes())
                .Union(new[] { type })
                .Where(
                    t => t.GetTypeInfo().IsGenericType
                         && (t.GetGenericTypeDefinition() == interfaceOrBaseType));
        }

        public static Type GetImplementationType(this Type type, Type interfaceOrBaseType)
            => type.GetGenericTypeImplementations(interfaceOrBaseType)
                .FirstOrDefault();

        public static bool TryGetImplementationType(this Type type, Type interfaceOrBaseType, out Type implementedInterfaceType)
            => (implementedInterfaceType = type.GetImplementationType(interfaceOrBaseType)) != null;

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.GetTypeInfo().BaseType;

            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
        }

        public static IEnumerable<Type> GetTypesInHierarchy(this Type type)
        {
            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] types)
        {
            types = types ?? new Type[0];

            return type.GetTypeInfo().DeclaredConstructors
                .SingleOrDefault(
                    c => !c.IsStatic
                         && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this Type type, string name)
        {
            do
            {
                var typeInfo = type.GetTypeInfo();
                var propertyInfo = typeInfo.GetDeclaredProperty(name);
                if (propertyInfo != null
                    && !(propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic)
                {
                    yield return propertyInfo;
                }
                type = typeInfo.BaseType;
            }
            while (type != null);
        }

        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type, string name)
        {
            // Do the whole hierarchy for properties first since looking for fields is slower.
            var currentType = type;
            do
            {
                var typeInfo = currentType.GetTypeInfo();
                var propertyInfo = typeInfo.GetDeclaredProperty(name);
                if (propertyInfo != null
                    && !(propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic)
                {
                    yield return propertyInfo;
                }
                currentType = typeInfo.BaseType;
            }
            while (currentType != null);

            currentType = type;
            do
            {
                var fieldInfo = currentType.GetRuntimeFields().FirstOrDefault(f => f.Name == name && !f.IsStatic);
                if (fieldInfo != null)
                {
                    yield return fieldInfo;
                }
                currentType = currentType.GetTypeInfo().BaseType;
            }
            while (currentType != null);
        }

        private static readonly Dictionary<Type, object> CommonTypeDictionary = new Dictionary<Type, object>
        {
            { typeof(int), default(int) },
            { typeof(Guid), default(Guid) },
            { typeof(DateTime), default(DateTime) },
            { typeof(DateTimeOffset), default(DateTimeOffset) },
            { typeof(long), default(long) },
            { typeof(bool), default(bool) },
            { typeof(double), default(double) },
            { typeof(short), default(short) },
            { typeof(float), default(float) },
            { typeof(byte), default(byte) },
            { typeof(char), default(char) },
            { typeof(uint), default(uint) },
            { typeof(ushort), default(ushort) },
            { typeof(ulong), default(ulong) },
            { typeof(sbyte), default(sbyte) }
        };

        public static object GetDefaultValue(this Type type)
        {
            if (!type.GetTypeInfo().IsValueType)
            {
                return null;
            }

            // A bit of perf code to avoid calling Activator.CreateInstance for common types and
            // to avoid boxing on every call. This is about 50% faster than just calling CreateInstance
            // for all value types.
            object value;
            return CommonTypeDictionary.TryGetValue(type, out value)
                ? value
                : Activator.CreateInstance(type);
        }

        public static IEnumerable<TypeInfo> GetConstructableTypes(this Assembly assembly)
            => assembly.GetLoadableDefinedTypes().Where(
                t => !t.IsAbstract
                     && !t.IsGenericTypeDefinition);

        public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo);
            }
        }
    }
}
