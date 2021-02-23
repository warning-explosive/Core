namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Singleton)]
    [Dependency(typeof(QueryReplyValidationPipeline))]
    internal class UnitOfWorkPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly EndpointIdentity _endpointIdentity;

        public UnitOfWorkPipeline(IMessagePipeline decoratee, EndpointIdentity endpointIdentity)
        {
            Decoratee = decoratee;
            _endpointIdentity = endpointIdentity;
        }

        public IMessagePipeline Decoratee { get; }

        public async Task Process(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token)
        {
            await using (context.WithinEndpointScope(new EndpointScope(_endpointIdentity, message), token).ConfigureAwait(false))
            {
                await Decoratee.Process(message, context, token).ConfigureAwait(false);
            }
        }
    }
}