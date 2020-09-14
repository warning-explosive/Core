namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Abstractions;
    using Basics;
    using SimpleInjector;

    internal abstract class AttributesVerifierBase : IConfigurationVerifier
    {
        private readonly Container _container;

        protected AttributesVerifierBase(Container container)
        {
            _container = container;
        }

        public void Verify()
        {
            VerifyInternal(Registered().ToList());
        }

        protected abstract void VerifyInternal(ICollection<Type> registered);

        protected IEnumerable<Type> Registered()
        {
            return _container
                  .GetCurrentRegistrations()
                  .Select(producer => producer.Registration.ImplementationType)
                  .Select(impl => impl.GenericTypeDefinitionOrSelf())
                  .Distinct();
        }

        protected IEnumerable<Type> ExtractAutoWiringServices(Type type)
        {
            return type.IncludedTypes().Where(i => typeof(IResolvable).IsAssignableFrom(i))
                       .Concat(type.ExtractGenericArgumentsAt(typeof(ICollectionResolvable<>), 0))
                       .Concat(type.ExtractGenericArgumentsAt(typeof(IExternalResolvable<>), 0))
                       .Distinct();
        }
    }
}