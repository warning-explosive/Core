namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Linq;
    using Api.Abstractions;
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
                    var resolvable = type.IsSubclassOfOpenGeneric(typeof(IResolvable<>)) && type != typeof(IResolvable<>);
                    var collectionResolvable = type.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)) && type != typeof(ICollectionResolvable<>);
                    var decorator = type.IsSubclassOfOpenGeneric(typeof(IDecorator<>)) && type != typeof(IDecorator<>);

                    return new ComponentInterfacesInfo(
                        type,
                        resolvable,
                        collectionResolvable,
                        decorator);
                })
                .Where(it => it.IsVerificationRequired())
                .Where(TypeHasCorrectlyDefinedInterfaces().Not())
                .Each(info => throw new InvalidOperationException($"Type {info.Type} has invalid {nameof(AutoRegistration)}.{nameof(AutoRegistration.Api)} interfaces configuration"));
        }

        private static Func<ComponentInterfacesInfo, bool> TypeHasCorrectlyDefinedInterfaces()
        {
            return info =>
            {
                var sum = info.IsResolvable().Bit()
                          + info.IsCollectionResolvable().Bit()
                          + info.IsDecorator().Bit();

                return sum == 1;
            };
        }

        private class ComponentInterfacesInfo
        {
            public ComponentInterfacesInfo(
                Type type,
                bool resolvable,
                bool collectionResolvable,
                bool decorator)
            {
                Type = type;
                Resolvable = resolvable;
                CollectionResolvable = collectionResolvable;
                Decorator = decorator;
            }

            public Type Type { get; }

            private bool Resolvable { get; }

            private bool CollectionResolvable { get; }

            private bool Decorator { get; }

            public bool IsVerificationRequired()
            {
                return Resolvable
                       || CollectionResolvable
                       || Decorator;
            }

            public bool IsResolvable()
            {
                return Resolvable
                       && !CollectionResolvable
                       && !Decorator;
            }

            public bool IsCollectionResolvable()
            {
                return !Resolvable
                       && CollectionResolvable
                       && !Decorator;
            }

            public bool IsDecorator()
            {
                return Decorator
                       && !CollectionResolvable;
            }
        }
    }
}