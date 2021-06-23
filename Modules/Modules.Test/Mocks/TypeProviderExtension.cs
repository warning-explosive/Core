namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class TypeProviderExtension : IResolvable
    {
        public TypeProviderExtension(IReadOnlyCollection<Type> ourTypes)
        {
            OurTypes = ourTypes;
        }

        public IReadOnlyCollection<Type> OurTypes { get; }
    }
}