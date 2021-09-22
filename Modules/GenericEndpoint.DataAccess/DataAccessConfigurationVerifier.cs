namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Exceptions;
    using CompositionRoot.Api.Abstractions;
    using Core.DataAccess.Api.DatabaseEntity;

    [Component(EnLifestyle.Singleton)]
    internal class DataAccessConfigurationVerifier : IConfigurationVerifier,
                                                     ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        public DataAccessConfigurationVerifier(
            ITypeProvider typeProvider,
            IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _typeProvider = typeProvider;
            _constructorResolutionBehavior = constructorResolutionBehavior;
        }

        public void Verify()
        {
            _typeProvider
                .OurTypes
                .Where(type => (typeof(IInlinedObject).IsAssignableFrom(type)
                                || type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
                               && type.IsConcreteType())
                .Where(type => !_constructorResolutionBehavior.TryGetConstructor(type, out _))
                .Each(type => throw new NotFoundException($"Type {type} should have one public constructor"));
        }
    }
}