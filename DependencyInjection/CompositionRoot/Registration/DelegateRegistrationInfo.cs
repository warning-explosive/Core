namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using Abstractions;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// DelegateRegistrationInfo
    /// </summary>
    public class DelegateRegistrationInfo : IRegistrationInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">EnLifestyle</param>
        public DelegateRegistrationInfo(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            Service = service.GenericTypeDefinitionOrSelf();
            InstanceProducer = instanceProducer;
            Lifestyle = lifestyle;
        }

        /// <inheritdoc />
        public Type Service { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle { get; }

        /// <summary>
        /// Instance producer
        /// </summary>
        public Func<object> InstanceProducer { get; }
    }
}