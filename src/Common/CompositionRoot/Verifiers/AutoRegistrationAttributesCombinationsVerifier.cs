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
            var exceptions = new List<Exception>();

            var infos = _typeProvider
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
                .Where(TypeHasCorrectlyDefinedAttributes().Not());

            foreach (var info in infos)
            {
                exceptions.Add(new InvalidOperationException($"Type {info.Type} has invalid {nameof(AutoRegistration)}.{nameof(AutoRegistration.Api)} attributes configuration"));
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private static Func<ComponentAttributesInfo, bool> TypeHasCorrectlyDefinedAttributes()
        {
            return info =>
            {
                var sum = info.IsNotComponent().Bit()
                          + info.IsComponent().Bit()
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

            public bool IsNotComponent()
            {
                return !Component
                       && !ManuallyRegistered
                       && !Unregistered
                       && !ComponentOverride
                       && !IsComponentCandidate(Type);
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
                    && (type.IsSubclassOfOpenGeneric(typeof(IResolvable<>))
                     || type.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>))
                     || type.IsSubclassOfOpenGeneric(typeof(IDecorator<>)));
            }
        }
    }
}