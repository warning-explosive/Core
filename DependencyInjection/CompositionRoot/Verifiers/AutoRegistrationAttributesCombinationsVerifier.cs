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
    internal class AutoRegistrationAttributesCombinationsVerifier : IConfigurationVerifier,
                                                                    ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public AutoRegistrationAttributesCombinationsVerifier(ITypeProvider typeProvider)
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
                    var componentOverride = type.HasAttribute<ComponentOverrideAttribute>();

                    return new ComponentAttributesInfo(type, component, manuallyRegistered, unregistered, componentOverride);
                })
                .Where(it => it.IsVerificationRequired())
                .Where(TypeHasCorrectlyDefinedAttributes().Not())
                .Each(info => throw new InvalidOperationException($"Type {info.Type} has invalid {nameof(AutoRegistration)}.{nameof(AutoRegistration.Api)} attributes configuration"));
        }

        private static Func<ComponentAttributesInfo, bool> TypeHasCorrectlyDefinedAttributes()
        {
            return info =>
            {
                var sum = info.IsComponent().Bit()
                          + info.IsManuallyRegisteredComponent().Bit()
                          + info.IsUnregisteredComponent().Bit()
                          + info.IsComponentOverride().Bit();

                return sum == 1;
            };
        }

        private class ComponentAttributesInfo
        {
            public ComponentAttributesInfo(
                Type type,
                bool component,
                bool manuallyRegistered,
                bool unregistered,
                bool componentOverride)
            {
                Type = type;
                Component = component;
                ManuallyRegistered = manuallyRegistered;
                Unregistered = unregistered;
                ComponentOverride = componentOverride;
            }

            public Type Type { get; }

            private bool Component { get; }

            private bool ManuallyRegistered { get; }

            private bool Unregistered { get; }

            private bool ComponentOverride { get; }

            public bool IsVerificationRequired()
            {
                return Component
                       || ManuallyRegistered
                       || Unregistered
                       || ComponentOverride;
            }

            public bool IsComponent()
            {
                return Component
                       && !ManuallyRegistered
                       && !Unregistered
                       && !ComponentOverride
                       && IsComponentCandidate(Type);
            }

            public bool IsManuallyRegisteredComponent()
            {
                return !Component
                       && ManuallyRegistered
                       && !Unregistered
                       && !ComponentOverride
                       && IsComponentCandidate(Type);
            }

            public bool IsUnregisteredComponent()
            {
                return !Component
                       && !ManuallyRegistered
                       && Unregistered
                       && !ComponentOverride
                       && IsComponentCandidate(Type);
            }

            public bool IsComponentOverride()
            {
                return !Component
                       && !ManuallyRegistered
                       && !Unregistered
                       && ComponentOverride
                       && IsComponentCandidate(Type);
            }

            private static bool IsComponentCandidate(Type type)
            {
                return type.IsConcreteType()
                       && (typeof(IResolvable).IsAssignableFrom(type)
                           || type.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>))
                           || type.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>))
                           || type.IsSubclassOfOpenGeneric(typeof(IDecorator<>)));
            }
        }
    }
}