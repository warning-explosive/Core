namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;

    /// <summary>
    /// Unregistered attribute
    /// For components that shouldn't be registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UnregisteredAttribute : Attribute
    {
    }
}