namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using Basics;
    using Extensions;
    using Registration;

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

        protected abstract void VerifyInternal(IReadOnlyCollection<Type> registeredComponents);

        protected static IEnumerable<Type> GetAutoRegistrationServices(Type type)
        {
            return type.ExtractGenericArgumentsAt(typeof(IResolvable<>))
                .Concat(type.ExtractGenericArgumentsAt(typeof(ICollectionResolvable<>)))
                .Distinct();
        }
    }
}