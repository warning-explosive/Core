namespace SpaceEngineers.Core.IntegrationTransport.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CompositionRoot;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;

    [ComponentOverride]
    internal class IntegrationTransportIntegrationTypeProvider : IIntegrationTypeProvider,
                                                                 IResolvable<IIntegrationTypeProvider>
    {
        private readonly ITypeProvider _typeProvider;

        private IReadOnlyCollection<Type>? _integrationMessageTypes;
        private IReadOnlyCollection<Type>? _repliesSubscriptions;

        public IntegrationTransportIntegrationTypeProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IReadOnlyCollection<Type> IntegrationMessageTypes()
        {
            _integrationMessageTypes ??= InitIntegrationMessageTypes();
            return _integrationMessageTypes;

            IReadOnlyCollection<Type> InitIntegrationMessageTypes()
            {
                return _typeProvider
                   .OurTypes
                   .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type)
                               && !type.IsMessageContractAbstraction())
                   .ToList();
            }
        }

        public IReadOnlyCollection<Type> EndpointCommands()
        {
            return Array.Empty<Type>();
        }

        public IReadOnlyCollection<Type> EndpointRequests()
        {
            return Array.Empty<Type>();
        }

        public IReadOnlyCollection<Type> RepliesSubscriptions()
        {
            _repliesSubscriptions ??= InitRepliesSubscriptions();
            return _repliesSubscriptions;

            IReadOnlyCollection<Type> InitRepliesSubscriptions()
            {
                return IntegrationMessageTypes()
                   .Where(type => type.IsReply()
                               && !type.IsMessageContractAbstraction())
                   .ToList();
            }
        }

        public IReadOnlyCollection<Type> EventsSubscriptions()
        {
            return Array.Empty<Type>();
        }
    }
}