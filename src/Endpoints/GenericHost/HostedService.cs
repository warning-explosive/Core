namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HostedService
    /// </summary>
    public class HostedService : IHostedService, IDisposable
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IHostedServiceRegistry _hostedServiceRegistry;
        private readonly IEnumerable<IHostedServiceObject> _objects;

        private readonly AsyncAutoResetEvent _sync;
        private Task _backgroundWorkersTask;

        private CancellationTokenSource? _cts;

        /// <summary> .cctor </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="logger">Logger</param>
        /// <param name="hostApplicationLifetime">IHostApplicationLifetime</param>
        /// <param name="hostedServiceRegistry">IHostedServiceRegistry</param>
        /// <param name="objects">HostedServiceObjects</param>
        public HostedService(
            string identifier,
            ILogger logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IHostedServiceRegistry hostedServiceRegistry,
            IEnumerable<IHostedServiceObject> objects)
        {
            Identifier = identifier;
            Logger = logger;

            _hostApplicationLifetime = hostApplicationLifetime;
            _hostedServiceRegistry = hostedServiceRegistry;
            _objects = objects;

            _sync = new AsyncAutoResetEvent(true);
            _backgroundWorkersTask = Task.CompletedTask;
        }

        private string Identifier { get; }

        private ILogger Logger { get; }

        private CancellationToken Token => _cts.Token;

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1725", Justification = "desired name")]
        public async Task StartAsync(CancellationToken token)
        {
            Logger.Information($"{nameof(HostedService)} {Identifier} is about to start");

            await _sync
               .WaitAsync(token)
               .ConfigureAwait(false);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var obj in _objects)
            {
                var runner = obj switch
                {
                    IHostedServiceStartupAction startupAction => RunStartupAction(startupAction),
                    IHostedServiceBackgroundWorker backgroundWorker => RunBackgroundWorker(backgroundWorker),
                    _ => throw new NotSupportedException(obj.GetType().FullName)
                };

                await runner.ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1725", Justification = "desired name")]
        public async Task StopAsync(CancellationToken token)
        {
            Logger.Information($"{nameof(HostedService)} {Identifier} is about to stop");

            if (_cts == null)
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

        /// <inheritdoc />
        public void Dispose()
        {
            Logger.Information($"{nameof(HostedService)} {Identifier} is about to dispose");

            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _backgroundWorkersTask.Wait(Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _sync.Set();
                _cts?.Dispose();
            }
        }

        private Func<Exception, CancellationToken, Task> OnUnhandledException(ILogger logger)
        {
            return async (exception, token) =>
            {
                logger.Critical(exception, $"{nameof(HostedService)} {Identifier} unhandled exception");

                await StopAsync(token).ConfigureAwait(false);
            };
        }

        private async Task Run<T>(T obj)
            where T : class, IHostedServiceObject
        {
            await obj
                .Run(Token)
                .TryAsync()
                .Catch<OperationCanceledException>()
                .Catch<Exception>(OnUnhandledException(Logger))
                .Invoke(Token)
                .ConfigureAwait(false);

            _hostedServiceRegistry.Enroll(obj);
        }

        private Task RunStartupAction(IHostedServiceStartupAction startupAction)
        {
            return Run(startupAction);
        }

        private Task RunBackgroundWorker(IHostedServiceBackgroundWorker startupAction)
        {
            _backgroundWorkersTask = Task.WhenAll(
                _backgroundWorkersTask,
                Run(startupAction));

            return Task.CompletedTask;
        }
    }
}