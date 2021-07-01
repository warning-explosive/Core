namespace SpaceEngineers.Core.GenericEndpoint.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Contract.Abstractions;
    using Contract.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier
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
             */
            Verify(_integrationTypeProvider.EndpointCommands());
            Verify(_integrationTypeProvider.EndpointQueries());
            Verify(_integrationTypeProvider.EndpointEvents());

            /*
             * 3. Message implements only one specialized interface (command, query, event or just message)
             */
            _integrationTypeProvider
                .IntegrationMessageTypes()
                .Where(ImplementsSeveralSpecializedInterfaces)
                .Each(type => throw new InvalidOperationException($"Message {type.FullName} must implement only one specialized interface (command, query, event or just message)"));

            /*
             * TODO: message handler should have got transient lifestyle
             */
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
                var handlerExists = _typeProvider
                    .OurTypes
                    .Any(type => type.IsClass
                                 && !type.IsAbstract
                                 && service.IsAssignableFrom(type));

                if (!handlerExists && !IsMessageAbstraction(message))
                {
                    throw new InvalidOperationException($"Message '{message.FullName}' should have message handler");
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
            var implements = Is(typeof(IIntegrationCommand).IsAssignableFrom(type))
                             + Is(type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)))
                             + Is(typeof(IIntegrationEvent).IsAssignableFrom(type));

            return implements > 1;
        }

        private static int Is(bool conditionResult)
        {
            return conditionResult ? 1 : 0;
        }
    }
}