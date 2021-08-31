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
    using Contract.Abstractions;
    using Contract.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier,
                                                          ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;

        public GenericEndpointConfigurationVerifier(
            ITypeProvider typeProvider,
            IIntegrationTypeProvider integrationTypeProvider)
        {
            _typeProvider = typeProvider;
            _integrationTypeProvider = integrationTypeProvider;
        }

        /// <inheritdoc />
        public void Verify()
        {
            /*
             * 1. Messages should be marked with OwnedByAttribute
             * 2. Endpoint should have message handler for owned messages
             * 3. Message handler should have transient lifestyle
             */
            Verify(_integrationTypeProvider.EndpointCommands());
            Verify(_integrationTypeProvider.EndpointQueries());
            Verify(_integrationTypeProvider.EndpointEvents());

            /*
             * 4. Message implements only one specialized interface (command, query, event or just message)
             */
            _integrationTypeProvider
                .IntegrationMessageTypes()
                .Where(ImplementsSeveralSpecializedInterfaces)
                .Each(type => throw new InvalidOperationException($"Message {type.FullName} must implement only one specialized interface (command, query, event or just message)"));
        }

        private void Verify(IEnumerable<Type> messageTypes)
        {
            foreach (var message in messageTypes)
            {
                if (!IsMessageAbstraction(message))
                {
                    _ = message.GetRequiredAttribute<OwnedByAttribute>();
                }

                var service = typeof(IMessageHandler<>).MakeGenericType(message);

                var messageHandlers = _typeProvider
                    .OurTypes
                    .Where(type => type.IsConcreteType()
                                   && service.IsAssignableFrom(type))
                    .ToList();

                if (!messageHandlers.Any()
                    && !IsMessageAbstraction(message))
                {
                    throw new InvalidOperationException($"Message '{message.FullName}' should have at least one message handler");
                }

                var lifestyleViolations = messageHandlers
                    .Where(messageHandler => messageHandler.GetRequiredAttribute<ComponentAttribute>().Lifestyle != EnLifestyle.Transient)
                    .ToList();

                if (lifestyleViolations.Any())
                {
                    throw new InvalidOperationException($"Message handlers {lifestyleViolations.ToString(", ", type => type.FullName)} should have transient lifestyle");
                }
            }
        }

        private static bool IsMessageAbstraction(Type type)
        {
            return type == typeof(IIntegrationMessage)
                   || type == typeof(IIntegrationCommand)
                   || type == typeof(IIntegrationEvent)
                   || typeof(IIntegrationQuery<>) == type.GenericTypeDefinitionOrSelf();
        }

        private static bool ImplementsSeveralSpecializedInterfaces(Type type)
        {
            var sum = typeof(IIntegrationCommand).IsAssignableFrom(type).Bit()
                      + type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)).Bit()
                      + typeof(IIntegrationEvent).IsAssignableFrom(type).Bit();

            return sum > 1;
        }
    }
}