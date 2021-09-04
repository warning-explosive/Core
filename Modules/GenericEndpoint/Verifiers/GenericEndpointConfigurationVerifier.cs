namespace SpaceEngineers.Core.GenericEndpoint.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using Contract.Abstractions;
    using Contract.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier,
                                                          ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly IRegistrationsContainer _registrations;

        public GenericEndpointConfigurationVerifier(
            IIntegrationTypeProvider integrationTypeProvider,
            IRegistrationsContainer registrations)
        {
            _integrationTypeProvider = integrationTypeProvider;
            _registrations = registrations;
        }

        /// <inheritdoc />
        public void Verify()
        {
            VerifyMessageInterfaces(_integrationTypeProvider.IntegrationMessageTypes());

            VerifyOwnedAttribute(_integrationTypeProvider.EndpointCommands());
            VerifyOwnedAttribute(_integrationTypeProvider.EndpointEvents());
            VerifyOwnedAttribute(_integrationTypeProvider.Replies());
            VerifyOwnedAttribute(_integrationTypeProvider.EndpointQueries());

            VerifyMessageHandlersLifestyle();

            VerifyHandlerExistence(_integrationTypeProvider.EndpointCommands());
            VerifyHandlerExistence(_integrationTypeProvider.EndpointEvents());
            VerifyHandlerExistence(_integrationTypeProvider.EndpointQueries());
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
                if (!messageType.IsConcreteType())
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

        private void VerifyMessageHandlersLifestyle()
        {
            var lifestyleViolations = _registrations
                .Collections()
                .Where(info => info.Implementation.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>))
                               && info.Lifestyle != EnLifestyle.Transient)
                .Select(info => info.Implementation)
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
                if (!messageType.IsConcreteType())
                {
                    continue;
                }

                var serviceType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                var messageHandlers = _registrations
                    .Collections()
                    .Where(info => serviceType.IsAssignableFrom(info.Implementation));

                if (!messageHandlers.Any())
                {
                    throw new InvalidOperationException($"Message '{messageType.FullName}' should have at least one message handler");
                }
            }
        }
    }
}