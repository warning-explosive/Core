namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using CompositionRoot.Exceptions;
    using CrossCuttingConcerns.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class HostedService : IHostedService, IDisposable
    {
        private static readonly AsyncAutoResetEvent Sync = new AsyncAutoResetEvent(true);

        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IEnumerable<IDependencyContainer> _dependencyContainers;
        private readonly IHostStartupActionsRegistry _hostStartupActionsRegistry;

        private CancellationTokenSource? _cts;
        private Task? _backgroundWorkersTask;

        public HostedService(
            Guid identifier,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory,
            IEnumerable<IDependencyContainer> dependencyContainers,
            IHostStartupActionsRegistry hostStartupActionsRegistry)
        {
            Identifier = identifier;

            Logger = loggerFactory.CreateLogger<HostedService>();

            _hostApplicationLifetime = hostApplicationLifetime;
            _dependencyContainers = dependencyContainers;
            _hostStartupActionsRegistry = hostStartupActionsRegistry;
        }

        public Guid Identifier { get; }

        private ILogger Logger { get; }

        private CancellationToken Token => _cts.Token;

        public async Task StartAsync(CancellationToken token)
        {
            await Sync
               .WaitAsync(token)
               .ConfigureAwait(false);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            var startupActions = _dependencyContainers
               .SelectMany(dependencyContainer => ExecutionExtensions
                   .Try(dependencyContainer, container => container.ResolveCollection<IHostStartupAction>())
                   .Catch<ComponentResolutionException>()
                   .Invoke(_ => Enumerable.Empty<IHostStartupAction>()));

            foreach (var action in startupActions)
            {
                await Run(action.Run, Token).ConfigureAwait(false);

                _hostStartupActionsRegistry.Enroll(action);
            }

            _backgroundWorkersTask = _dependencyContainers
               .SelectMany(dependencyContainer => ExecutionExtensions
                   .Try(dependencyContainer, container => container.ResolveCollection<IHostBackgroundWorker>())
                   .Catch<ComponentResolutionException>()
                   .Invoke(_ => Enumerable.Empty<IHostBackgroundWorker>()))
               .Select(worker => Run(worker.Run, Token))
               .WhenAll();
        }

        public async Task StopAsync(CancellationToken token)
        {
            Logger.Information("Application is being shut down");

            if (_backgroundWorkersTask == null)
            {
                _hostApplicationLifetime.StopApplication();

                return;
            }

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
                   .WhenAny(_backgroundWorkersTask, Task.Delay(Timeout.InfiniteTimeSpan, token))
                   .Unwrap()
                   .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            _hostApplicationLifetime.StopApplication();
        }

        public void Dispose()
        {
            Logger.Information($"{nameof(HostedService)} is being disposed");

            try
            {
                _cts?.Cancel();
                _backgroundWorkersTask?.Wait(Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Sync.Set();
                _cts?.Dispose();
            }
        }

        private Task Run(Func<CancellationToken, Task> action, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(action)
                .Catch<Exception>(OnUnhandledException(Logger))
                .Invoke(token);
        }

        private Func<Exception, CancellationToken, Task> OnUnhandledException(ILogger logger)
        {
            return async (exception, token) =>
            {
                logger.Critical(exception, $"Hosted service {Identifier} unhandled exception");

                await StopAsync(token).ConfigureAwait(false);
            };
        }
    }
}