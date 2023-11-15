namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing.Host
{
    using Basics;
    using Registrations;
    using SpaceEngineers.Core.GenericEndpoint.Host.Builder;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// With sql event sourcing
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithSqlEventSourcing(this IEndpointBuilder builder)
        {
            builder.CheckMultipleCalls(nameof(WithSqlEventSourcing));

            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(EventSourcing))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericDomain), nameof(GenericDomain.EventSourcing), nameof(GenericDomain.EventSourcing.Sql)))
            };

            return builder
                .ModifyContainerOptions(options => options
                    .WithPluginAssemblies(assemblies)
                    .WithManualRegistrations(new EventSourcingHostStartupActionsManualRegistration()));
        }
    }
}