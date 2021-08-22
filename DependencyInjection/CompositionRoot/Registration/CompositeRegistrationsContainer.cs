namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent]
    internal class CompositeRegistrationsContainer : IRegistrationsContainer
    {
        private readonly IRegistrationsContainer _overrides;
        private readonly IEnumerable<IRegistrationsContainer> _registrations;

        public CompositeRegistrationsContainer(IRegistrationsContainer overrides, params IRegistrationsContainer[] registrations)
        {
            _overrides = overrides;
            _registrations = registrations;
        }

        public IEnumerable<(Type Type, object Instance)> Singletons()
        {
            return Aggregate(container => container.Singletons());
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            return Aggregate(container => container.Resolvable());
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            return Aggregate(container => container.Delegates());
        }

        public IEnumerable<ServiceRegistrationInfo> Collections()
        {
            return Aggregate(container => container.Collections());
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            return Aggregate(container => container.Decorators());
        }

        public IEnumerable<T> Aggregate<T>(Func<IRegistrationsContainer, IEnumerable<T>> accessor)
        {
            return _registrations.SelectMany(accessor);
        }

        internal IRegistrationsContainer ApplyOverrides()
        {
            throw new NotImplementedException();
        }
    }
}