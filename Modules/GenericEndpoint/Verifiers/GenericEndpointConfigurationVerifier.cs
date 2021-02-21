namespace SpaceEngineers.Core.GenericEndpoint.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Contract.Abstractions;
    using Contract.Attributes;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class GenericEndpointConfigurationVerifier : IConfigurationVerifier
    {
        private readonly IIntegrationTypeProvider _integrationTypeProvider;

        public GenericEndpointConfigurationVerifier(
            IIntegrationTypeProvider integrationTypeProvider)
        {
            _integrationTypeProvider = integrationTypeProvider;
        }

        /// <inheritdoc />
        public void Verify()
        {
            /*
             * 1. Messages must be marked with OwnedByAttribute
             */
            Owned(_integrationTypeProvider.EndpointCommands());
            Owned(_integrationTypeProvider.EndpointQueries());
            Owned(_integrationTypeProvider.EndpointQueries());

            /*
             * 2. Message implements only one specialized interface (command, query, event or just message)
             */
            _integrationTypeProvider
                .IntegrationMessageTypes()
                .Where(ImplementsSeveralSpecializedInterfaces)
                .Each(type => throw new InvalidOperationException($"Message {type.FullName} must implement only one specialized interface (command, query, event or just message)"));
        }

        private static void Owned(IEnumerable<Type> messageTypes)
        {
            messageTypes.Each(command => command.HasAttribute<OwnedByAttribute>());
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