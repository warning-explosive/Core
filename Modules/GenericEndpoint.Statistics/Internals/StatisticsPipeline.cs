namespace SpaceEngineers.Core.GenericEndpoint.Statistics.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using StatisticsEndpoint.Contract.Messages;

    [Component(EnLifestyle.Singleton)]
    [Dependency(typeof(GenericEndpoint.Pipeline.QueryReplyValidationPipeline))]
    [Dependent(typeof(GenericEndpoint.Pipeline.UnitOfWorkPipeline))]
    internal class StatisticsPipeline : IMessagePipelineStep, IMessagePipeline
    {
        public StatisticsPipeline(IMessagePipeline decoratee)
        {
            Decoratee = decoratee;
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

        private static Task OnSuccess(IAdvancedIntegrationContext context, CancellationToken token)
        {
            var command = new CaptureMessageStatistics(context.Message);
            return context.Send(command, token);
        }

        private static Func<Exception, CancellationToken, Task> OnError(IAdvancedIntegrationContext context)
        {
            return async (exception, token) =>
            {
                var command = new CaptureMessageStatistics(context.Message)
                {
                    Exception = exception
                };

                await context.Send(command, token).ConfigureAwait(false);
                throw exception.Rethrow();
            };
        }
    }
}