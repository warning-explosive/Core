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
        private readonly IEnumerable<IHostStartupAction> _startupActions;
        private readonly IEnumerable<IHostBackgroundWorker> _backgroundWorkers;

        private CancellationTokenSource? _cts;
        private Task? _backgroundWorkersTask;

        public HostedService(
            ILoggerFactory loggerFactory,
            IEnumerable<IHostStartupAction> startupActions,
            IEnumerable<IHostBackgroundWorker> backgroundWorkers)
        {
            Logger = loggerFactory.CreateLogger<HostedService>();

            _startupActions = startupActions;
            _backgroundWorkers = backgroundWorkers;
        }

        private ILogger<HostedService> Logger { get; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var action in _startupActions.OrderByDependencyAttribute(action => action.GetType()))
            {
                await action.Run(Token).ConfigureAwait(false);
            }

            _backgroundWorkersTask = _backgroundWorkers
                .Select(worker => worker.Run(Token))
                .WhenAll();
        }

        public async Task StopAsync(CancellationToken token)
        {
            if (_backgroundWorkersTask == null)
            {
                // Stop called without start
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _cts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task
                    .WhenAny(_backgroundWorkersTask, Task.Delay(Timeout.InfiniteTimeSpan, token))
                    .ConfigureAwait(false);
            }
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
    }
}