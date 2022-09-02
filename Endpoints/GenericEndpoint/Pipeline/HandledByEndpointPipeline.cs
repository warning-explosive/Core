namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Contract;
    using Messaging.MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(UnitOfWorkPipeline))]
    internal class HandledByEndpointPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly EndpointIdentity _endpointIdentity;

        public HandledByEndpointPipeline(
            IMessagePipeline decoratee,
            EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
            Decoratee = decoratee;
        }

        public IMessagePipeline Decoratee { get; }

        public async Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            try
            {
                await Decoratee
                   .Process(producer, context, token)
                   .ConfigureAwait(false);
            }
            finally
            {
                context.Message.OverwriteHeader(new HandledBy(_endpointIdentity));
            }
        }
    }
}