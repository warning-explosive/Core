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
    internal class AutoWiringAttributesCombinationsVerifier : IConfigurationVerifier,
                                                              ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public AutoWiringAttributesCombinationsVerifier(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            _typeProvider
                .OurTypes
                .Select(type =>
                {
                    var component = type.HasAttribute<ComponentAttribute>();
                    var manuallyRegistered = type.HasAttribute<ManuallyRegisteredComponentAttribute>();
                    var unregistered = type.HasAttribute<UnregisteredComponentAttribute>();

                    return new ComponentAttributesInfo(type, component, manuallyRegistered, unregistered);
                })
                .Where(TypeHasCorrectlyDefinedAttributes().Not())
                .Each(info => throw new InvalidOperationException($"Type {info.Type} has invalid {nameof(AutoRegistration)}.{nameof(AutoRegistration.Api)} attributes configuration"));

            static Func<ComponentAttributesInfo, bool> TypeHasCorrectlyDefinedAttributes()
            {
                return info => info.IsNotComponent()
                               || info.IsComponent()
                               || info.IsManuallyRegisteredComponent()
                               || info.IsUnregisteredComponent();
            }
        }

        private class ComponentAttributesInfo
        {
            public ComponentAttributesInfo(
                Type type,
                bool component,
                bool manuallyRegistered,
                bool unregistered)
            {
                Type = type;
                Component = component;
                ManuallyRegistered = manuallyRegistered;
                Unregistered = unregistered;
            }

            public Type Type { get; }

            private bool Component { get; }

            private bool ManuallyRegistered { get; }

            private bool Unregistered { get; }

            public bool IsNotComponent()
            {
                return !Component
                       && !ManuallyRegistered
                       && !Unregistered;
            }

            public bool IsComponent()
            {
                return Component
                       && !ManuallyRegistered
                       && !Unregistered;
            }

            public bool IsManuallyRegisteredComponent()
            {
                return !Component
                       && ManuallyRegistered
                       && !Unregistered;
            }

            public bool IsUnregisteredComponent()
            {
                return !Component
                       && !ManuallyRegistered
                       && Unregistered;
            }
        }
    }
}