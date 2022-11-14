namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using Contract;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Extensions;
    using Messaging;
    using Microsoft.Extensions.Logging;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpoint : IExecutableEndpoint,
                                     IGenericEndpoint,
                                     IResolvable<IExecutableEndpoint>,
                                     IResolvable<IGenericEndpoint>
    {
        private readonly ILogger _logger;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly AsyncManualResetEvent _ready;
        private readonly ConcurrentDictionary<Guid, Task> _runningHandlers;

        private CancellationTokenSource? _cts;

        public GenericEndpoint(
            ILogger logger,
            EndpointIdentity endpointIdentity,
            IDependencyContainer dependencyContainer)
        {
            _logger = logger;
            _endpointIdentity = endpointIdentity;

            DependencyContainer = dependencyContainer;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new ConcurrentDictionary<Guid, Task>();
        }

        public IDependencyContainer DependencyContainer { get; }

        private CancellationToken Token => _cts.Token;

        public Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            _logger.Information($"{_endpointIdentity} has been started");

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
            Task? runningHandler = default;

            try
            {
                runningHandler = DependencyContainer
                    .ResolveGeneric(typeof(IMessageHandlerExecutor<>), message.ReflectedType)
                    .CallMethod(nameof(IMessageHandlerExecutor<IIntegrationMessage>.Invoke))
                    .WithArguments(message, Token)
                    .Invoke<Task>();

                _runningHandlers.Add(executionId, runningHandler);

                await runningHandler.ConfigureAwait(false);
            }
            finally
            {
                if (runningHandler != null)
                {
                    _runningHandlers.Remove(executionId, out _);
                }
            }
        }
    }
}