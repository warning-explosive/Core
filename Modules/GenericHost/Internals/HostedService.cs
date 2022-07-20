namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class HostedService : IHostedService, IDisposable
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IEnumerable<IHostStartupAction> _startupActions;
        private readonly IEnumerable<IHostBackgroundWorker> _backgroundWorkers;

        private CancellationTokenSource? _cts;
        private Task? _backgroundWorkersTask;

        public HostedService(
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory,
            IEnumerable<IHostStartupAction> startupActions,
            IEnumerable<IHostBackgroundWorker> backgroundWorkers)
        {
            Id = Guid.NewGuid();

            Logger = loggerFactory.CreateLogger<HostedService>();

            _hostApplicationLifetime = hostApplicationLifetime;
            _startupActions = startupActions;
            _backgroundWorkers = backgroundWorkers;
        }

        private Guid Id { get; }

        private ILogger Logger { get; }

        private CancellationToken Token => _cts.Token;

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var action in _startupActions.OrderByDependencyAttribute(action => action.GetType()))
            {
                await Run(action.Run, Token).ConfigureAwait(false);
            }

            _backgroundWorkersTask = _backgroundWorkers
                .Select(worker => Run(worker.Run, Token))
                .WhenAll();
        }

        public async Task StopAsync(CancellationToken token)
        {
            if (_backgroundWorkersTask == null)
            {
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
                logger.Critical(exception, $"Hosted service {Id} unhandled exception");

                await StopAsync(token).ConfigureAwait(false);
            };
        }
    }
}