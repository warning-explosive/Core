namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using Abstractions;

    internal interface IMemberExtensions : IResolvable
    {
        object GetPropertyValue(object target, string propertyName);

        object GetFieldValue(object target, string fieldName);
    }
}