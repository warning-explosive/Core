namespace SpaceEngineers.Core.IntegrationTransport.Host.StartupActions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using BackgroundWorkers;
    using Basics.Attributes;
    using Basics.Primitives;
    using GenericHost;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [After(typeof(IntegrationTransportHostedServiceBackgroundWorker))]
    internal class IntegrationTransportHostedServiceStartupAction : IHostedServiceStartupAction,
                                                                    ICollectionResolvable<IHostedServiceObject>,
                                                                    ICollectionResolvable<IHostedServiceStartupAction>,
                                                                    IResolvable<IntegrationTransportHostedServiceStartupAction>
    {
        private readonly Task _subscription;

        public IntegrationTransportHostedServiceStartupAction(IExecutableIntegrationTransport transport)
        {
            _subscription = WaitUntilTransportIsNotRunning(transport);
        }

        public Task Run(CancellationToken token)
        {
            return _subscription;
        }

        private static async Task WaitUntilTransportIsNotRunning(IExecutableIntegrationTransport integrationTransport)
        {
            var tcs = new TaskCompletionSource<object?>();

            var subscription = MakeSubscription(tcs);

            using (Disposable.Create((integrationTransport, subscription), Subscribe, Unsubscribe))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<IntegrationTransportStatusChangedEventArgs> MakeSubscription(
                TaskCompletionSource<object?> tcs)
            {
                return (_, args) =>
                {
                    if (args.CurrentStatus == EnIntegrationTransportStatus.Running)
                    {
                        tcs.TrySetResult(default!);
                    }
                };
            }

            static void Subscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged += subscription;
            }

            static void Unsubscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged -= subscription;
            }
        }
    }
}