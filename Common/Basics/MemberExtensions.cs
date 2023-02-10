namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Exceptions;

    /// <summary>
    /// Extensions for operations with class members
    /// </summary>
    public static class MemberExtensions
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;

        private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";

        private static readonly Type[] IsExternalInitTypes =
            new[]
            {
                TypeExtensions.FindType("System.Private.CoreLib System.Runtime.CompilerServices.IsExternalInit"),
                TypeExtensions.FindType("SpaceEngineers.Core.Basics System.Runtime.CompilerServices.IsExternalInit")
            };

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="memberInfo">MemberInfo</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        public static TAttribute GetRequiredAttribute<TAttribute>(this MemberInfo memberInfo)
            where TAttribute : Attribute
        {
            return memberInfo
                .GetCustomAttributes<TAttribute>()
                .InformativeSingleOrDefault(Amb)
                .EnsureNotNull(() => new AttributeRequiredException(typeof(TAttribute), memberInfo));

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="memberInfo">MemberInfo</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        public static TAttribute? GetAttribute<TAttribute>(this MemberInfo memberInfo)
            where TAttribute : Attribute
        {
            return memberInfo
                .GetCustomAttributes<TAttribute>()
                .InformativeSingleOrDefault(Amb);

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Does the specified type has an attribute
        /// </summary>
        /// <param name="memberInfo">MemberInfo</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute existence</returns>
        public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo)
            where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes<TAttribute>().Any();
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="methodInfo">MethodInfo</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        public static TAttribute GetRequiredAttribute<TAttribute>(this MethodInfo methodInfo)
            where TAttribute : Attribute
        {
            return methodInfo
                .GetCustomAttributes<TAttribute>()
                .InformativeSingleOrDefault(Amb)
                .EnsureNotNull(() => new AttributeRequiredException(typeof(TAttribute), methodInfo));

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="methodInfo">MethodInfo</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        public static TAttribute? GetAttribute<TAttribute>(this MethodInfo methodInfo)
            where TAttribute : Attribute
        {
            return methodInfo
                .GetCustomAttributes<TAttribute>()
                .InformativeSingleOrDefault(Amb);

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Does the specified type has an attribute
        /// </summary>
        /// <param name="methodInfo">MethodInfo</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute existence</returns>
        public static bool HasAttribute<TAttribute>(this MethodInfo methodInfo)
            where TAttribute : Attribute
        {
            return methodInfo.GetCustomAttributes<TAttribute>().Any();
        }

        /// <summary>
        /// Extract GenericMethodDefinition or return argument method
        /// </summary>
        /// <param name="method">MethodInfo</param>
        /// <returns>GenericMethodDefinition or argument method</returns>
        public static MethodInfo GenericMethodDefinitionOrSelf(this MethodInfo method)
        {
            return method.IsGenericMethod
                ? method.GetGenericMethodDefinition()
                : method;
        }

        /// <summary>
        /// Does method have external access (public or internal)
        /// </summary>
        /// <param name="method">MethodBase</param>
        /// <returns>True if method has external access (public or internal)</returns>
        public static bool IsAccessible(this MethodBase method)
        {
            return (method.IsPublic || method.IsAssembly)
                && !(method.IsPrivate || method.IsFamilyOrAssembly);
        }

        /// <summary>
        /// Does property getter have external access (public or internal)
        /// </summary>
        /// <param name="property">PropertyInfo</param>
        /// <returns>True if property getter has external access (public or internal)</returns>
        public static bool GetIsAccessible(this PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(true);
            return getMethod != null && getMethod.IsAccessible();
        }

        /// <summary>
        /// Does property setter have external access (public or internal)
        /// </summary>
        /// <param name="property">PropertyInfo</param>
        /// <returns>True if property setter has external access (public or internal)</returns>
        public static bool SetIsAccessible(this PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod != null && setMethod.IsAccessible();
        }

        /// <summary>
        /// Does property represents equality contract generated by compiler
        /// </summary>
        /// <param name="property">PropertyInfo</param>
        /// <returns>True if property represents equality contract generated by compiler</returns>
        public static bool IsEqualityContract(this PropertyInfo property)
        {
            return property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase)
                   && property.GetMethod?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
        }

        /// <summary>
        /// Check that object has property
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Property value</returns>
        public static bool HasProperty(this object target, string propertyName)
        {
            return target
                  .GetType()
                  .GetProperty(propertyName, Flags) != null;
        }

        /// <summary>
        /// Get property value from target object
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Property value</returns>
        public static object GetPropertyValue(this object target, string propertyName)
        {
            return target
                  .GetType()
                  .GetProperty(propertyName, Flags | BindingFlags.GetProperty)
                  .GetValue(target);
        }

        /// <summary>
        /// Get property value from target object
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="propertyName">Name of the property</param>
        /// <typeparam name="TProperty">TProperty type-argument</typeparam>
        /// <returns>Property value</returns>
        public static TProperty GetPropertyValue<TProperty>(this object target, string propertyName)
        {
            return (TProperty)target.GetPropertyValue(propertyName);
        }

        /// <summary>
        /// Check that object has field
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Field value</returns>
        public static bool HasField(this object target, string fieldName)
        {
            return HasField(target.GetType(), fieldName);
        }

        /// <summary>
        /// Check that object has field
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Field value</returns>
        public static bool HasField(this Type type, string fieldName)
        {
            return type.GetField(fieldName, Flags) != null;
        }

        /// <summary>
        /// Get field value from target object
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Field value</returns>
        public static object GetFieldValue(this object target, string fieldName)
        {
            return GetFieldValue(target.GetType(), target, fieldName);
        }

        /// <summary>
        /// Get field value from target object
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="fieldName">Name of the field</param>
        /// <typeparam name="TField">TField type-argument</typeparam>
        /// <returns>Field value</returns>
        public static TField GetFieldValue<TField>(this object target, string fieldName)
        {
            return (TField)target.GetFieldValue(fieldName);
        }

        /// <summary>
        /// Get field value from target object
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="target">Target object</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Field value</returns>
        public static object GetFieldValue(this Type type, object target, string fieldName)
        {
            return type.HasField(fieldName)
                ? type.GetField(fieldName, Flags).GetValue(target)
                : throw new InvalidOperationException($"Type '{type}' doesn't contain field '{fieldName}'");
        }

        /// <summary>
        /// Get field value from target object
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="target">Target object</param>
        /// <param name="fieldName">Name of the field</param>
        /// <typeparam name="TField">TField type-argument</typeparam>
        /// <returns>Field value</returns>
        public static TField GetFieldValue<TField>(this Type type, object target, string fieldName)
        {
            return (TField)type.GetFieldValue(target, fieldName);
        }

        /// <summary>
        /// Get dictionary of object property values
        /// Key - property name, Value - property value
        /// </summary>
        /// <param name="target">Target object</param>
        /// <returns>Property dictionary</returns>
        public static Dictionary<string, object?> ToPropertyDictionary(this object target)
        {
            return target.GetType()
                .GetProperties(Flags | BindingFlags.GetProperty)
                .ToDictionary(
                    info => info.Name,
                    info => (object?)info.GetValue(target),
                    StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Does property have the initializer (init modifier);
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>Property have the initializer or not</returns>
        public static bool HasInitializer(this PropertyInfo propertyInfo)
        {
            return propertyInfo.SetMethod != default
                && propertyInfo.SetMethod.ReturnParameter != null
                && propertyInfo.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any(IsExternalInitTypes.Contains);
        }

        /// <summary>
        /// Is property nullable
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>Property is nullable or not</returns>
        public static bool IsNullable(this PropertyInfo propertyInfo)
        {
            return IsNullable(propertyInfo, info => info.PropertyType, info => info.GetCustomAttributes());
        }

        /// <summary>
        /// Is field nullable
        /// </summary>
        /// <param name="fieldInfo">FieldInfo</param>
        /// <returns>Field is nullable or not</returns>
        public static bool IsNullable(this FieldInfo fieldInfo)
        {
            return IsNullable(fieldInfo, info => info.FieldType, info => info.GetCustomAttributes());
        }

        private static bool IsNullable<T>(
            this T memberInfo,
            Func<T, Type> typeAccessor,
            Func<T, IEnumerable<Attribute>> attributesAccessor)
            where T : MemberInfo
        {
            var isNullableValueType = typeAccessor(memberInfo).IsNullable();

            if (isNullableValueType)
            {
                return true;
            }

            var typeName = $"{memberInfo.ReflectedType.Assembly.GetName().Name} {NullableAttributeFullName}";

            return TypeExtensions.TryFindType(typeName, out var nullableAttributeType)
                && attributesAccessor(memberInfo).Any(nullableAttributeType.IsInstanceOfType);
        }
    }
}