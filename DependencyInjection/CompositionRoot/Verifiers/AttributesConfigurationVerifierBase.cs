namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using Basics;
    using Extensions;

    internal abstract class AttributesConfigurationVerifierBase : IConfigurationVerifier
    {
        private readonly IRegistrationsContainer _registrations;

        protected AttributesConfigurationVerifierBase(IRegistrationsContainer registrations)
        {
            _registrations = registrations;
        }

        public void Verify()
        {
            VerifyInternal(_registrations.RegisteredComponents().ToList());
        }

        protected abstract void VerifyInternal(ICollection<Type> registered);

        protected static IEnumerable<Type> ExtractAutoWiringServices(Type type)
        {
            return type.IncludedTypes().Where(t => typeof(IResolvable).IsAssignableFrom(t))
                       .Concat(type.ExtractGenericArgumentsAt(typeof(ICollectionResolvable<>), 0))
                       .Concat(type.ExtractGenericArgumentsAt(typeof(IExternalResolvable<>), 0))
                       .Distinct();
        }
    }
}