namespace SpaceEngineers.Core.GenericDomain.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;

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
            return _typeProvider
                .OurTypes
                .Where(type => type.IsClass
                               && !type.IsAbstract
                               && typeof(IEntity).IsAssignableFrom(type));
        }
    }
}