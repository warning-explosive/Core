namespace SpaceEngineers.Core.Modules.Test.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Statistics.Internals;
    using Mocks;

    internal class GenericEndpointStatisticsPipelineOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IMessagePipeline, StatisticsPipeline, StatisticsPipelineMock>(EnLifestyle.Singleton);
        }
    }
}