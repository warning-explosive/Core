namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract.Abstractions;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpoint : IRunnableEndpoint,
                                     IExecutableEndpoint
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly ConcurrentDictionary<Guid, Task> _runningHandlers;
        private CancellationTokenSource? _cts;

        public GenericEndpoint(
            IDependencyContainer dependencyContainer,
            IEnumerable<IEndpointInitializer> initializers)
        {
            DependencyContainer = dependencyContainer;
            Initializers = initializers;

            _ready = new AsyncManualResetEvent(false);
            _runningHandlers = new ConcurrentDictionary<Guid, Task>();
        }

        public IDependencyContainer DependencyContainer { get; }

        public IEnumerable<IEndpointInitializer> Initializers { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var initializer in Initializers)
            {
                await initializer.Initialize(Token).ConfigureAwait(false);
            }

            _ready.Set();
        }

        public async Task StopAsync(CancellationToken token)
        {
            _ready.Reset();

            try
            {
                // Signal cancellation to the executing handlers
                _cts.Cancel();
            }
            finally
            {
                // Wait until completes all running handlers or the stop token triggers
                await Task
                    .WhenAny(_runningHandlers.Values.WhenAll(), Task.Delay(Timeout.InfiniteTimeSpan, token))
                    .ConfigureAwait(false);
            }
        }

        public async Task ExecuteMessageHandlers(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(Token).ConfigureAwait(false);

            var executionId = Guid.NewGuid();
            Task? runningHandler = default;

            try
            {
                runningHandler = DependencyContainer
                    .ResolveGeneric(typeof(IMessageHandlerExecutor<>), message.ReflectedType)
                    .CallMethod(nameof(IMessageHandlerExecutor<IIntegrationMessage>.Invoke))
                    .WithArguments(message, token)
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