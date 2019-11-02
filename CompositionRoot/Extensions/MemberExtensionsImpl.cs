namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System.Reflection;
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class MemberExtensionsImpl : IMemberExtensions
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public object GetPropertyValue(object target, string propertyName)
        {
            return target
                  .GetType()
                  .GetProperty(propertyName, Flags)
                  .GetValue(target);
        }

        public object GetFieldValue(object target, string fieldName)
        {
            return target
                  .GetType()
                  .GetField(fieldName, Flags)
                  .GetValue(target);
        }
    }
}