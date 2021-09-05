namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Attributes;
    using GenericEndpoint.Abstractions;

    [ComponentOverride]
    [Dependency(typeof(GenericEndpoint.Pipeline.QueryReplyValidationPipeline))]
    [Dependent(typeof(GenericEndpoint.Pipeline.UnitOfWorkPipeline))]
    internal class StatisticsPipelineMock : IMessagePipelineStep, IMessagePipeline
    {
        private readonly MessagesCollector _collector;

        public StatisticsPipelineMock(
            IMessagePipeline decoratee,
            MessagesCollector collector)
        {
            Decoratee = decoratee;
            _collector = collector;
        }

        public IMessagePipeline Decoratee { get; }

        public Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync((producer, context), Process)
                .Catch<Exception>(OnError(context))
                .Invoke(token);
        }

        private async Task Process(
            (Func<IAdvancedIntegrationContext, CancellationToken, Task>, IAdvancedIntegrationContext) state,
            CancellationToken token)
        {
            var (producer, context) = state;
            await Decoratee.Process(producer, context, token).ConfigureAwait(false);
            await OnSuccess(context, token).ConfigureAwait(false);
        }

        private Task OnSuccess(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return _collector.Collect(context.Message, null, token);
        }

        private Func<Exception, CancellationToken, Task> OnError(IAdvancedIntegrationContext context)
        {
            return async (exception, token) =>
            {
                await _collector.Collect(context.Message, exception, token).ConfigureAwait(false);
                throw exception.Rethrow();
            };
        }
    }
}