namespace SpaceEngineers.Core.GenericEndpoint.Statistics.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Attributes;

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

        public Task Process(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(() => Decoratee.Process(context, token))
                .Invoke(ex => OnError(context, ex, token));
        }

        private Task OnError(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}