namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host.Registrations
{
    using System;
    using System.Diagnostics.Metrics;
    using CompositionRoot.Registration;
    using Contract;
    using GenericHost;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class TelemetryManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate(() =>
                {
                    var endpointIdentity = container
                        .Advanced
                        .DependencyContainer
                        .Resolve<EndpointIdentity>();

                    if (!container
                            .Advanced
                            .DependencyContainer
                            .Resolve<IFrameworkDependenciesProvider>()
                            .GetRequiredService<ITelemetryProvider>()
                            .TryGetTracerProvider(endpointIdentity, out var tracerProvider))
                    {
                        throw new InvalidOperationException("Method .UseOpenTelemetry() should be called in order to setup telemetry");
                    }

                    return tracerProvider.GetTracer(endpointIdentity.LogicalName, endpointIdentity.Version);
                },
                EnLifestyle.Singleton);

            container.Advanced.RegisterDelegate(() =>
                {
                    var endpointIdentity = container
                        .Advanced
                        .DependencyContainer
                        .Resolve<EndpointIdentity>();

                    if (!container
                            .Advanced
                            .DependencyContainer
                            .Resolve<IFrameworkDependenciesProvider>()
                            .GetRequiredService<ITelemetryProvider>()
                            .TryGetMeterProvider(endpointIdentity, out var meterProvider))
                    {
                        throw new InvalidOperationException("Method .UseOpenTelemetry() should be called in order to setup telemetry");
                    }

                    return new Meter(endpointIdentity.LogicalName, endpointIdentity.Version);
                },
                EnLifestyle.Singleton);
        }
    }
}