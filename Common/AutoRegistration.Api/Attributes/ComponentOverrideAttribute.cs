namespace SpaceEngineers.Core.AutoRegistration.Api.Attributes
{
    using System;

    /// <summary>
    /// Defines component override that will be skipped in auto-registration phase and should be registered manually
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentOverrideAttribute : Attribute
    {
    }
}