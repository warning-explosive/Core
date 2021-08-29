namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host.Internals;
    using InMemoryIntegrationTransport.Host.Internals;
    using Microsoft.Extensions.Logging;

    internal class ModulesTestManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var name = AssembliesExtensions.BuildName(nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test));

            container.Advanced.RegisterFactory<ILogger>(() =>
                {
                    using var loggerFactory = new LoggerFactory();
                    return loggerFactory.CreateLogger(name);
                },
                EnLifestyle.Singleton);

            var endpointIdentity = new EndpointIdentity(name, 0);
            new GenericEndpointIdentityManualRegistration(endpointIdentity).Register(container);

            new InMemoryIntegrationTransportManualRegistration().Register(container);

            new ExtendedTypeProviderManualRegistration(Array.Empty<Type>()).Register(container);
        }
    }
}