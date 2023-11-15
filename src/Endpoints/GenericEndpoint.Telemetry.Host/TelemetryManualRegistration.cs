namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host
{
    using System.Diagnostics.Metrics;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using Contract;
    using GenericHost;
    using OpenTelemetry.Trace;

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

                    return container
                        .Advanced
                        .DependencyContainer
                        .Resolve<IFrameworkDependenciesProvider>()
                        .GetRequiredService<TracerProvider>()
                        .GetTracer(endpointIdentity.LogicalName, endpointIdentity.Version);
                },
                EnLifestyle.Singleton);

            container.Advanced.RegisterDelegate(() =>
                {
                    var endpointIdentity = container
                        .Advanced
                        .DependencyContainer
                        .Resolve<EndpointIdentity>();

                    return new Meter(endpointIdentity.LogicalName, endpointIdentity.Version);
                },
                EnLifestyle.Singleton);
        }
    }
}