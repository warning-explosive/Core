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
    [Dependency(typeof(GenericEndpoint.Pipeline.UnitOfWorkPipeline))]
    [Dependent(typeof(GenericEndpoint.Pipeline.ErrorHandlingPipeline))]
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
                .TryAsync(async () =>
                {
                    await Decoratee.Process(producer, context, token).ConfigureAwait(false);
                    await OnSuccess(context, token).ConfigureAwait(false);
                })
                .Catch<Exception>()
                .Invoke(ex => OnError(context, ex, token));
        }

        private static Task OnSuccess(IAdvancedIntegrationContext context, CancellationToken token)
        {
            var command = new CaptureMessageStatistics(context.Message);
            return context.Send(command, token);
        }

        private static Task OnError(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            var command = new CaptureMessageStatistics(context.Message) { Exception = exception };
            return context.Send(command, token);
        }
    }
}