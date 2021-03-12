namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Singleton)]
    [Dependency(typeof(QueryReplyValidationPipeline))]
    internal class UnitOfWorkPipeline : IMessagePipelineStep, IMessagePipeline
    {
        public UnitOfWorkPipeline(IMessagePipeline decoratee)
        {
            Decoratee = decoratee;
        }

        public IMessagePipeline Decoratee { get; }

        public async Task Process(IExtendedIntegrationContext context, CancellationToken token)
        {
            await using (await context.UnitOfWork.StartTransaction(context, token).ConfigureAwait(false))
            {
                await Decoratee.Process(context, token).ConfigureAwait(false);

                context.UnitOfWork.SaveChanges();
            }
        }
    }
}