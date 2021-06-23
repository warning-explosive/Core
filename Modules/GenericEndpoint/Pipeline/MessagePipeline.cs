namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Contract.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class MessagePipeline : IMessagePipeline
    {
        private readonly IDependencyContainer _dependencyContainer;

        public MessagePipeline(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Process(IAdvancedIntegrationContext context, CancellationToken token)
        {
            var handlerServiceType = typeof(IMessageHandler<>).MakeGenericType(context.Message.ReflectedType);

            await _dependencyContainer
                .Resolve(handlerServiceType)
                .CallMethod(nameof(IMessageHandler<IIntegrationMessage>.Handle))
                .WithArguments(context.Message.Payload, context, token)
                .Invoke<Task>()
                .ConfigureAwait(false);
        }
    }
}