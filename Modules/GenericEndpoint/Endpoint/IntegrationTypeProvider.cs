namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;
    using Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationTypeProvider : IIntegrationTypeProvider,
                                             IResolvable<IIntegrationTypeProvider>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IRegistrationsContainer _registrations;

        private IReadOnlyCollection<Type>? _integrationMessageTypes;
        private IReadOnlyCollection<Type>? _endpointCommands;
        private IReadOnlyCollection<Type>? _endpointQueries;
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

        public IEnumerable<Type> IntegrationMessageTypes()
        {
            _integrationMessageTypes ??= InitIntegrationMessageTypes();
            return _integrationMessageTypes;

            IReadOnlyCollection<Type> InitIntegrationMessageTypes()
            {
                return _typeProvider
                    .OurTypes
                    .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type))
                    .ToList();
            }
        }

        public IEnumerable<Type> EndpointCommands()
        {
            _endpointCommands ??= InitEndpointCommands();
            return _endpointCommands;

            IReadOnlyCollection<Type> InitEndpointCommands()
            {
                return _typeProvider
                    .OurTypes
                    .Where(type => typeof(IIntegrationCommand).IsAssignableFrom(type)
                                   && !type.IsMessageContractAbstraction()
                                   && OwnedByCurrentEndpoint(type)
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        public IEnumerable<Type> EndpointQueries()
        {
            _endpointQueries ??= InitEndpointQueries();
            return _endpointQueries;

            IReadOnlyCollection<Type> InitEndpointQueries()
            {
                return _typeProvider
                    .OurTypes
                    .Where(type => type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>))
                                   && !type.IsMessageContractAbstraction()
                                   && OwnedByCurrentEndpoint(type)
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        public IEnumerable<Type> RepliesSubscriptions()
        {
            _repliesSubscriptions ??= InitRepliesSubscriptions();
            return _repliesSubscriptions;

            IReadOnlyCollection<Type> InitRepliesSubscriptions()
            {
                return _typeProvider
                    .OurTypes
                    .Where(type => typeof(IIntegrationReply).IsAssignableFrom(type)
                                   && !type.IsMessageContractAbstraction()
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        public IEnumerable<Type> EventsSubscriptions()
        {
            _eventsSubscriptions ??= InitEventsSubscriptions();
            return _eventsSubscriptions;

            IReadOnlyCollection<Type> InitEventsSubscriptions()
            {
                return _typeProvider
                    .OurTypes
                    .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type)
                                   && type.HasMessageHandler(_registrations))
                    .ToList();
            }
        }

        private bool OwnedByCurrentEndpoint(Type type)
        {
            return type.GetRequiredAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase);
        }
    }
}