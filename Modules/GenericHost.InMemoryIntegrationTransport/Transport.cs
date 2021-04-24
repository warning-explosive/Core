namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport
{
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Core.GenericHost.Abstractions;
    using CrossCuttingConcerns;
    using Internals;
    using Registrations;

    /// <summary>
    /// In-memory integration transport entry point
    /// </summary>
    public static class Transport
    {
        /// <summary>
        /// Builds in-memory integration transport implementation
        /// </summary>
        /// <returns>IIntegrationTransport instance</returns>
        public static IIntegrationTransport InMemoryIntegrationTransport()
        {
            var containerOptions = new DependencyContainerOptions
            {
                ManualRegistrations = new IManualRegistration[]
                {
                    new InMemoryIntegrationTransportRegistration(),
                    new CrossCuttingConcernsManualRegistration()
                }
            };

            return DependencyContainer
                .CreateExactlyBounded(
                    containerOptions,
                    typeof(InMemoryIntegrationTransport).Assembly,
                    typeof(GenericHost).Assembly)
                .Resolve<InMemoryIntegrationTransport>();
        }
    }
}