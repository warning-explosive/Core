namespace SpaceEngineers.Core.Basics
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions for operations with class members
    /// </summary>
    public static class MemberExtensions
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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
                  .GetProperty(propertyName, Flags)
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
        /// Get field value from target object
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Field value</returns>
        public static object GetFieldValue(this object target, string fieldName)
        {
            return target
                  .GetType()
                  .GetField(fieldName, Flags)
                  .GetValue(target);
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
        /// Get dictionary of object property values
        /// Key - property name, Value - property value
        /// </summary>
        /// <param name="target">Target object</param>
        /// <returns>Property dictionary</returns>
        public static IDictionary<string, object> ToPropertyDictionary(this object target)
        {
            return target.GetType()
                         .GetProperties(Flags)
                         .ToDictionary(prop => prop.Name,
                                       prop => prop.GetValue(target));
        }
    }
}