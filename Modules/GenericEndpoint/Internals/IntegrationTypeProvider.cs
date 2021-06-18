namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
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
                .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type)
                               && typeof(IIntegrationMessage) != type
                               && typeof(IIntegrationCommand) != type
                               && typeof(IIntegrationEvent) != type
                               && typeof(IIntegrationQuery<>) != type.GenericTypeDefinitionOrSelf());
        }

        public IEnumerable<Type> EndpointCommands()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationCommand).IsAssignableFrom(type)
                               && typeof(IIntegrationCommand) != type
                               && type.GetRequiredAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Type> EndpointQueries()
        {
            return _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>))
                               && typeof(IIntegrationQuery<>) != type.GenericTypeDefinitionOrSelf()
                               && type.GetRequiredAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Type> EndpointEvents()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type)
                               && typeof(IIntegrationEvent) != type
                               && type.GetRequiredAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Type> EndpointSubscriptions()
        {
            return _typeProvider
                .OurTypes
                .Where(type => type.IsClass
                               && !type.IsAbstract
                               && type.IsSubclassOfOpenGeneric(typeof(IMessageHandler<>)))
                .SelectMany(type => type.ExtractGenericArgumentsAt(typeof(IMessageHandler<>), 0))
                .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type)
                               && !type.GetRequiredAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase))
                .Distinct();
        }
    }
}