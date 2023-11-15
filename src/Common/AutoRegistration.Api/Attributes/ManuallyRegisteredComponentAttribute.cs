namespace SpaceEngineers.Core.AutoRegistration.Api.Attributes
{
    using System;

    /// <summary>
    /// Defines application component that should be manually registered into dependency container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ManuallyRegisteredComponentAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="justification">Justification</param>
        public ManuallyRegisteredComponentAttribute(string justification)
        {
            Justification = justification;
        }

        /// <summary>
        /// Justification
        /// </summary>
        public string Justification { get; }
    }
}