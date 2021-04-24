namespace SpaceEngineers.Core.AutoWiring.Api.Attributes
{
    using System;
    using Enumerations;

    /// <summary>
    /// Component attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="lifestyle">Lifestyle</param>
        /// <param name="registrationKind">Component registration kind</param>
        public ComponentAttribute(
            EnLifestyle lifestyle,
            EnComponentRegistrationKind registrationKind = EnComponentRegistrationKind.AutomaticallyRegistered)
        {
            Lifestyle = lifestyle;
            RegistrationKind = registrationKind;
        }

        /// <summary>
        /// Service lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <summary>
        /// Component registration kind
        /// </summary>
        public EnComponentRegistrationKind RegistrationKind { get; }
    }
}