namespace SpaceEngineers.Core.AutoRegistration.Api.Attributes
{
    using System;

    /// <summary>
    /// Defines application component that shouldn't be registered into dependency container neither automatically nor manually
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UnregisteredComponentAttribute : Attribute
    {
    }
}