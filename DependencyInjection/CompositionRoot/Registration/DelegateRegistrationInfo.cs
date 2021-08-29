namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// DelegateRegistrationInfo
    /// </summary>
    public class DelegateRegistrationInfo
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

        /// <summary>
        /// Service
        /// </summary>
        public Type Service { get; }

        /// <summary>
        /// Lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <summary>
        /// Instance producer
        /// </summary>
        public Func<object> InstanceProducer { get; }
    }
}