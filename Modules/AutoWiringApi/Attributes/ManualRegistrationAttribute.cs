namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;

    /// <summary>
    /// Manual registration attribute
    /// For components that must be registered by hand // TODO: verify it on after registration phase
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ManualRegistrationAttribute : Attribute
    {
    }
}