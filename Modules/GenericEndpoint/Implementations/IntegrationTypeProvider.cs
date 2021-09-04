namespace SpaceEngineers.Core.GenericEndpoint.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationTypeProvider : IIntegrationTypeProvider
    {
        private readonly ITypeProvider _typeProvider;
        private readonly EndpointIdentity _endpointIdentity;

        public IntegrationTypeProvider(
            EndpointIdentity endpointIdentity,
            ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _endpointIdentity = endpointIdentity;
        }

        public IEnumerable<Type> IntegrationMessageTypes()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type));
        }

        public IEnumerable<Type> EndpointCommands()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationCommand).IsAssignableFrom(type)
                               && (IsMessageAbstraction(type) || OwnedByCurrentEndpoint(type)));
        }

        public IEnumerable<Type> EndpointQueries()
        {
            return _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>))
                               && (IsMessageAbstraction(type) || OwnedByCurrentEndpoint(type)));
        }

        public IEnumerable<Type> EndpointEvents()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type)
                               && (IsMessageAbstraction(type) || OwnedByCurrentEndpoint(type)));
        }

        public IEnumerable<Type> Replies()
        {
            return _typeProvider
                .AllLoadedTypes
                .Where(type => typeof(IIntegrationReply).IsAssignableFrom(type));
        }

        public IEnumerable<Type> EndpointSubscriptions()
        {
            return _typeProvider
                .OurTypes
                .Where(type => type.IsConcreteType()
                               && type.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>)))
                .SelectMany(type => type.ExtractGenericArgumentsAt(typeof(IMessageHandler<>), 0))
                .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type))
                .Distinct();
        }

        private static bool IsMessageAbstraction(Type type)
        {
            return type == typeof(IIntegrationMessage)
                   || type == typeof(IIntegrationCommand)
                   || type == typeof(IIntegrationEvent)
                   || typeof(IIntegrationQuery<>) == type.GenericTypeDefinitionOrSelf();
        }

        private bool OwnedByCurrentEndpoint(Type type)
        {
            return (typeof(IIntegrationCommand).IsAssignableFrom(type)
                    || typeof(IIntegrationEvent).IsAssignableFrom(type)
                    || type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)))
                   && type.GetRequiredAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase);
        }
    }
}