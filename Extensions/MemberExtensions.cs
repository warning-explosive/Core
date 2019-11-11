namespace SpaceEngineers.Core.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class MemberExtensions
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static object GetPropertyValue(this object target, string propertyName)
        {
            return target
                  .GetType()
                  .GetProperty(propertyName, Flags)
                  .GetValue(target);
        }

        public static object GetFieldValue(this object target, string fieldName)
        {
            return target
                  .GetType()
                  .GetField(fieldName, Flags)
                  .GetValue(target);
        }

        public static IDictionary<string, object> ToPropertyDictionary(this object target)
        {
            return target.GetType()
                         .GetProperties(Flags)
                         .ToDictionary(prop => prop.Name,
                                       prop => prop.GetValue(target));
        }
    }
}