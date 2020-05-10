namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;

    /// <summary>
    /// Unregistered attribute
    /// For components that shouldn't be registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UnregisteredAttribute : Attribute
    {
    }
}