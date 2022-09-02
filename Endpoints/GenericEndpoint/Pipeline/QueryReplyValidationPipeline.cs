namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using Messaging.Extensions;
    using UnitOfWork;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(UnitOfWorkPipeline))]
    internal class QueryReplyValidationPipeline : IMessagePipelineStep, IMessagePipeline
    {
        private readonly IDependencyContainer _dependencyContainer;

        public QueryReplyValidationPipeline(
            IMessagePipeline decoratee,
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
            Decoratee = decoratee;
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

            if (context.Message.IsQuery())
            {
                var repliesCount = _dependencyContainer
                   .Resolve<IOutboxStorage>()
                   .All()
                   .Count(message => message.IsReplyOnQuery(context.Message));

                switch (repliesCount)
                {
                    case < 1: throw new InvalidOperationException("Message handler should reply to the query");
                    case > 1: throw new InvalidOperationException("Message handler should reply to the query only once");
                }
            }
        }
    }
}