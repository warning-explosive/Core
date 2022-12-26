namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Contract;
    using Contract.Abstractions;
    using Contract.Extensions;
    using Extensions;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationTypeProvider : IIntegrationTypeProvider,
                                             IResolvable<IIntegrationTypeProvider>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IRegistrationsContainer _registrations;

        private IReadOnlyCollection<Type>? _integrationMessageTypes;
        private IReadOnlyCollection<Type>? _endpointCommands;
        private IReadOnlyCollection<Type>? _endpointRequests;
        private IReadOnlyCollection<Type>? _repliesSubscriptions;
        private IReadOnlyCollection<Type>? _eventsSubscriptions;

        public IntegrationTypeProvider(
            EndpointIdentity endpointIdentity,
            ITypeProvider typeProvider,
            IRegistrationsContainer registrations)
        {
            _endpointIdentity = endpointIdentity;
            _typeProvider = typeProvider;
            _registrations = registrations;
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
            _endpointCommands ??= InitEndpointCommands();
            return _endpointCommands;

            IReadOnlyCollection<Type> InitEndpointCommands()
            {
                return IntegrationMessageTypes()
                    .Where(type => type.IsCommand()
                                   && !type.IsMessageContractAbstraction()
                                   && type.IsOwnedByEndpoint(_endpointIdentity)
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        public IReadOnlyCollection<Type> EndpointRequests()
        {
            _endpointRequests ??= InitEndpointRequests();
            return _endpointRequests;

            IReadOnlyCollection<Type> InitEndpointRequests()
            {
                return IntegrationMessageTypes()
                    .Where(type => type.IsRequest()
                                   && !type.IsMessageContractAbstraction()
                                   && type.IsOwnedByEndpoint(_endpointIdentity)
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        public IReadOnlyCollection<Type> RepliesSubscriptions()
        {
            _repliesSubscriptions ??= InitRepliesSubscriptions();
            return _repliesSubscriptions;

            IReadOnlyCollection<Type> InitRepliesSubscriptions()
            {
                return IntegrationMessageTypes()
                    .Where(type => type.IsReply()
                                   && !type.IsMessageContractAbstraction()
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        public IReadOnlyCollection<Type> EventsSubscriptions()
        {
            _eventsSubscriptions ??= InitEventsSubscriptions();
            return _eventsSubscriptions;

            IReadOnlyCollection<Type> InitEventsSubscriptions()
            {
                return IntegrationMessageTypes()
                    .Where(type => type.IsEvent()
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }
    }
}