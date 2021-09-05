namespace SpaceEngineers.Core.Modules.Test.Extensions
{
    using CompositionRoot;
    using Mocks;
    using Overrides;
    using Registrations;

    internal static class HostExtensions
    {
        public static DependencyContainerOptions WithStatisticsOverride(
            this DependencyContainerOptions options,
            MessagesCollector collector)
        {
            return options
                .WithManualRegistrations(new MessagesCollectorInstanceManualRegistration(collector))
                .WithOverrides(new GenericEndpointStatisticsPipelineOverride());
        }
    }
}