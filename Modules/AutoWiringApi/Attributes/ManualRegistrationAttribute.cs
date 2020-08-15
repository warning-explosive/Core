namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;

    /// <summary>
    /// Manual registration attribute
    /// For components that must be registered by hand
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ManualRegistrationAttribute : Attribute
    {
    }
}