namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CompositeRegistrationsContainer : IRegistrationsContainer
    {
        private readonly IEnumerable<IRegistrationsContainer> _containers;

        internal CompositeRegistrationsContainer(params IRegistrationsContainer[] containers)
        {
            _containers = containers;
        }

        public IEnumerable<(Type, object)> Singletons()
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
            return _containers.SelectMany(accessor);
        }
    }
}