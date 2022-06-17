namespace SpaceEngineers.Core.GenericEndpoint.Tracing.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Contract;
    using CrossCuttingConcerns.Json;
    using Extensions;
    using GenericEndpoint.Pipeline;
    using Messaging.Abstractions;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(GenericEndpoint.Pipeline.UnitOfWorkPipeline))]
    [Dependent(typeof(GenericEndpoint.Pipeline.ErrorHandlingPipeline))]
    internal class TracingPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationMessageFactory _integrationMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;

        public TracingPipeline(
            IMessagePipeline decoratee,
            EndpointIdentity endpointIdentity,
            IIntegrationMessageFactory integrationMessageFactory,
            IJsonSerializer jsonSerializer)
        {
            Decoratee = decoratee;

            _endpointIdentity = endpointIdentity;
            _integrationMessageFactory = integrationMessageFactory;
            _jsonSerializer = jsonSerializer;
        }

        public IMessagePipeline Decoratee { get; }

        public async Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            await Decoratee
               .Process(producer, context, token)
               .ConfigureAwait(false);

            await context.CaptureTrace(
                    _endpointIdentity,
                    default,
                    _integrationMessageFactory,
                    _jsonSerializer,
                    token)
               .ConfigureAwait(false);
        }
    }
}