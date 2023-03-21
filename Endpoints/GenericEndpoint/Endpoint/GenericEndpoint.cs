namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Logging;
    using Messaging;
    using Microsoft.Extensions.Logging;
    using Pipeline;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpoint : IExecutableEndpoint,
                                     IGenericEndpoint,
                                     IResolvable<IExecutableEndpoint>,
                                     IResolvable<IGenericEndpoint>
    {
        private readonly ILogger _logger;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IMessageHandlerMiddlewareComposite _messageHandlerMiddleware;

        private readonly AsyncManualResetEvent _ready;
        private readonly ConcurrentDictionary<Guid, Task> _runningHandlers;

        private CancellationTokenSource? _cts;

        public GenericEndpoint(
            ILogger logger,
            IDependencyContainer dependencyContainer,
            IMessageHandlerMiddlewareComposite messageHandlerMiddleware)
        {
            _logger = logger;
            _dependencyContainer = dependencyContainer;
            _messageHandlerMiddleware = messageHandlerMiddleware;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new ConcurrentDictionary<Guid, Task>();
        }

        private CancellationToken Token => _cts.Token;

        public Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            _logger.Information("Endpoint has been started");

            _ready.Set();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken token)
        {
            _ready.Reset();

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                await Task
                   .WhenAny(_runningHandlers.Values.WhenAll(), Task.Delay(Timeout.InfiniteTimeSpan, token))
                   .Unwrap()
                   .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task ExecuteMessageHandler(IntegrationMessage message)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            var executionId = Guid.NewGuid();

            var runningHandler = RunMessageHandler(
                _dependencyContainer,
                _messageHandlerMiddleware,
                message,
                Token);

            using (Disposable.Create(_runningHandlers, Add(executionId, runningHandler), Remove(executionId)))
            {
                await runningHandler.ConfigureAwait(false);
            }

            static Action<ConcurrentDictionary<Guid, Task>> Add(
                Guid executionId,
                Task runningHandler)
            {
                return runningHandlers => runningHandlers.Add(executionId, runningHandler);
            }

            static Action<ConcurrentDictionary<Guid, Task>> Remove(
                Guid executionId)
            {
                return runningHandlers => runningHandlers.Remove(executionId, out _);
            }

            static async Task RunMessageHandler(
                IDependencyContainer dependencyContainer,
                IMessageHandlerMiddlewareComposite messageHandlerMiddleware,
                IntegrationMessage message,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var exclusiveContext = dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(message);

                    var messageHandler = dependencyContainer.ResolveGeneric(typeof(IMessageHandler<>), message.ReflectedType);

                    await messageHandlerMiddleware
                        .Handle(exclusiveContext, Next(messageHandler), token)
                        .ConfigureAwait(false);
                }
            }

            static Func<IAdvancedIntegrationContext, CancellationToken, Task> Next(object messageHandler)
            {
                return (context, token) => messageHandler
                    .CallMethod(nameof(IMessageHandler<IIntegrationMessage>.Handle))
                    .WithArguments(context.Message.Payload, token)
                    .Invoke<Task>();
            }
        }
    }
}