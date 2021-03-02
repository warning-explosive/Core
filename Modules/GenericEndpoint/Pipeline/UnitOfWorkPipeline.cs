namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
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

        public async Task Process(IExtendedIntegrationContext context, CancellationToken token)
        {
            var builder = new AsyncUnitOfWorkBuilder<EndpointIdentity>();

            await using var contextScope = context.WithinEndpointScope(builder).ConfigureAwait(false);
            {
                var unitOfWork = builder.OpenTransaction(_endpointIdentity, token);
                await using (unitOfWork.ConfigureAwait(false))
                {
                    await Decoratee.Process(context, token).ConfigureAwait(false);

                    unitOfWork.SaveChanges();
                }
            }
        }
    }
}