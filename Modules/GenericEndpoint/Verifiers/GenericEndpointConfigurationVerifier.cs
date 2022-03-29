namespace SpaceEngineers.Core.GenericEndpoint.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using CompositionRoot.Api.Extensions;
    using Contract.Abstractions;
    using Contract.Attributes;
    using Endpoint;
    using Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier,
                                                          ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly IRegistrationsContainer _registrations;

        public GenericEndpointConfigurationVerifier(
            IConstructorResolutionBehavior constructorResolutionBehavior,
            IIntegrationTypeProvider integrationTypeProvider,
            IRegistrationsContainer registrations)
        {
            _constructorResolutionBehavior = constructorResolutionBehavior;
            _integrationTypeProvider = integrationTypeProvider;
            _registrations = registrations;
        }

        /// <inheritdoc />
        public void Verify()
        {
            VerifyMessageInterfaces(_integrationTypeProvider.IntegrationMessageTypes());
            VerifyOwnedAttribute(_integrationTypeProvider.IntegrationMessageTypes());
            VerifyConstructors(_integrationTypeProvider.IntegrationMessageTypes());
            VerifyDeserializationRequirement(_integrationTypeProvider.IntegrationMessageTypes());

            VerifyMessageHandlersLifestyle();

            VerifyHandlerExistence(_integrationTypeProvider.EndpointCommands().Where(type => type.IsConcreteType()));
            VerifyHandlerExistence(_integrationTypeProvider.EndpointQueries().Where(type => type.IsConcreteType()));
        }

        private static void VerifyMessageInterfaces(IEnumerable<Type> messageTypes)
        {
            messageTypes
                .Where(ImplementsSeveralSpecializedInterfaces)
                .Each(type => throw new InvalidOperationException($"Message {type.FullName} must implement only one specialized interface (command, query, event or just message)"));

            static bool ImplementsSeveralSpecializedInterfaces(Type type)
            {
                var sum = typeof(IIntegrationCommand).IsAssignableFrom(type).Bit()
                          + typeof(IIntegrationEvent).IsAssignableFrom(type).Bit()
                          + typeof(IIntegrationReply).IsAssignableFrom(type).Bit()
                          + type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)).Bit();

                return sum > 1;
            }
        }

        private static void VerifyOwnedAttribute(IEnumerable<Type> messageTypes)
        {
            foreach (var messageType in messageTypes)
            {
                if (messageType.IsMessageContractAbstraction())
                {
                    continue;
                }

                if (typeof(IIntegrationReply).IsAssignableFrom(messageType))
                {
                    if (messageType.HasAttribute<OwnedByAttribute>())
                    {
                        throw new InvalidOperationException($"Reply should not have {nameof(OwnedByAttribute)}");
                    }
                }
                else
                {
                    _ = messageType.GetRequiredAttribute<OwnedByAttribute>();
                }
            }
        }

        private void VerifyConstructors(IEnumerable<Type> messageTypes)
        {
            messageTypes
               .Where(messageType => messageType.IsConcreteType())
               .Where(messageType => !_constructorResolutionBehavior.TryGetConstructor(messageType, out _))
               .Each(messageType => throw new InvalidOperationException($"Message {messageType.FullName} should have one public constructor"));
        }

        private static void VerifyDeserializationRequirement(IEnumerable<Type> messageTypes)
        {
            messageTypes
               .Where(messageType => messageType.IsConcreteType())
               .SelectMany(messageType => messageType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
               .Where(property => !(property.HasInitializer() && property.SetMethod.IsPublic))
               .Each(property => throw new InvalidOperationException($"Property {property.ReflectedType.FullName}.{property.Name} should have public initializer (init modifier) so as to be deserializable"));
        }

        private void VerifyMessageHandlersLifestyle()
        {
            var lifestyleViolations = _registrations
                .Collections()
                .Where(info => info.Lifestyle != EnLifestyle.Transient)
                .RegisteredComponents()
                .Where(info => info.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>)))
                .ToList();

            if (lifestyleViolations.Any())
            {
                throw new InvalidOperationException($"Message handlers {lifestyleViolations.ToString(", ", type => type.FullName)} should have transient lifestyle");
            }
        }

        private void VerifyHandlerExistence(IEnumerable<Type> messageTypes)
        {
            foreach (var messageType in messageTypes)
            {
                if (!messageType.HasMessageHandler(_registrations))
                {
                    throw new InvalidOperationException($"Message '{messageType.FullName}' should have at least one message handler");
                }
            }
        }
    }
}