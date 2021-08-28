namespace SpaceEngineers.Core.GenericDomain.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class DomainTypeProvider : IDomainTypeProvider
    {
        private readonly ITypeProvider _typeProvider;

        public DomainTypeProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerable<Type> Entities()
        {
            return DomainTypes(typeof(IEntity));
        }

        public IEnumerable<Type> Aggregates()
        {
            return DomainTypes(typeof(IAggregate));
        }

        public IEnumerable<Type> ValueObjects()
        {
            return DomainTypes(typeof(IValueObject));
        }

        public IEnumerable<Type> EnumerationObjects()
        {
            return DomainTypes(typeof(EnumerationObject));
        }

        private IEnumerable<Type> DomainTypes(Type service)
        {
            return _typeProvider
                .OurTypes
                .Where(type => type.IsConcreteType()
                               && service.IsAssignableFrom(type));
        }
    }
}