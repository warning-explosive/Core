namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using Basics;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Attributes;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class IntegrationTypesProvider : IIntegrationTypesProvider
    {
        private readonly ITypeProvider _typeProvider;
        private readonly EndpointIdentity _endpointIdentity;

        public IntegrationTypesProvider(
            EndpointIdentity endpointIdentity,
            ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _endpointIdentity = endpointIdentity;
        }

        public IEnumerable<Type> EndpointCommands()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationCommand).IsAssignableFrom(type)
                               && typeof(IIntegrationCommand) != type
                               && type.GetAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Type> EndpointEvents()
        {
            return _typeProvider
                .OurTypes
                .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type)
                               && typeof(IIntegrationEvent) != type
                               && type.GetAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase));
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
                               && !type.GetAttribute<OwnedByAttribute>().EndpointName.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase))
                .Distinct();
        }
    }
}