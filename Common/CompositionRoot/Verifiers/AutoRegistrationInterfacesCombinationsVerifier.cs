namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class AutoRegistrationInterfacesCombinationsVerifier : IConfigurationVerifier,
                                                                    ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public AutoRegistrationInterfacesCombinationsVerifier(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            _typeProvider
                .OurTypes
                .Select(type =>
                {
                    var resolvable = type.ExtractGenericArgumentsAt(typeof(IResolvable<>)).ToList();
                    var collectionResolvable = type.ExtractGenericArgumentsAt(typeof(ICollectionResolvable<>)).ToList();
                    var decorator = type.ExtractGenericArgumentsAt(typeof(IDecorator<>)).ToList();

                    return new ComponentInterfacesInfo(
                        type,
                        resolvable,
                        collectionResolvable,
                        decorator);
                })
                .Where(info => info.IsVerificationRequired())
                .Where(info => !info.TypeHasCorrectlyDefinedInterfaces())
                .Each(info => throw new InvalidOperationException($"Type {info.Type} has invalid {nameof(AutoRegistration)}.{nameof(AutoRegistration.Api)} interfaces configuration"));
        }

        private class ComponentInterfacesInfo
        {
            public ComponentInterfacesInfo(
                Type type,
                IReadOnlyCollection<Type> resolvable,
                IReadOnlyCollection<Type> collectionResolvable,
                IReadOnlyCollection<Type> decorator)
            {
                Type = type;
                Resolvable = resolvable;
                CollectionResolvable = collectionResolvable;
                Decorator = decorator;
            }

            public Type Type { get; }

            private IReadOnlyCollection<Type> Resolvable { get; }

            private IReadOnlyCollection<Type> CollectionResolvable { get; }

            private IReadOnlyCollection<Type> Decorator { get; }

            public bool IsVerificationRequired()
            {
                return Resolvable.Any()
                       || CollectionResolvable.Any()
                       || Decorator.Any();
            }

            public bool TypeHasCorrectlyDefinedInterfaces()
            {
                return !Resolvable
                   .Intersect(CollectionResolvable)
                   .Intersect(Decorator).Any();
            }
        }
    }
}