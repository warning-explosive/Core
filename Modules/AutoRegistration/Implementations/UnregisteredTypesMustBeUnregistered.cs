namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using Basics;
    using SimpleInjector;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class UnregisteredTypesMustBeUnregistered : AttributesVerifierBase
    {
        private readonly ITypeProvider _typeProvider;

        public UnregisteredTypesMustBeUnregistered(Container container, ITypeProvider typeProvider)
            : base(container)
        {
            _typeProvider = typeProvider;
        }

        protected override void VerifyInternal(ICollection<Type> registered)
        {
            _typeProvider
               .OurTypes
               .Where(type => type.HasAttribute<UnregisteredAttribute>())
               .SelectMany(implementation => ExtractAutoWiringServices(implementation).Select(service => (service, implementation)))
               .Where(pair => registered.Contains(pair.implementation)
                           || registered.Contains(pair.service))
               .Each(pair => throw new InvalidOperationException($"{pair.implementation.FullName} marked with the {nameof(UnregisteredAttribute)} but represented in container as {pair.service.FullName}"));
        }
    }
}