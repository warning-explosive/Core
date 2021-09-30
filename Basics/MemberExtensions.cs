namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions for operations with class members
    /// </summary>
    public static class MemberExtensions
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;

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
        public static Dictionary<string, (Type Type, object? Value)> ToPropertyDictionary(this object target)
        {
            return target.GetType()
                .GetProperties(Flags | BindingFlags.GetProperty)
                .ToDictionary(
                    info => info.Name,
                    info =>
                    {
                        var value = (object?)info.GetValue(target);
                        return (value?.GetType() ?? info.PropertyType, value);
                    },
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}