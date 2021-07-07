namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Services;
    using Basics;
    using Extensions;
    using SimpleInjector;

    internal abstract class AttributesConfigurationVerifierBase : IConfigurationVerifier
    {
        private readonly Container _container;

        protected AttributesConfigurationVerifierBase(Container container)
        {
            _container = container;
        }

        public void Verify()
        {
            VerifyInternal(_container.RegisteredComponents().ToList());
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