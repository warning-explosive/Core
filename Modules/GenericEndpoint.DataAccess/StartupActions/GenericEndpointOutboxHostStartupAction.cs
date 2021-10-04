namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.StartupActions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract;
    using Core.DataAccess.Api.Transaction;
    using Deduplication;
    using GenericDomain.Api.Abstractions;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;

    internal class GenericEndpointOutboxHostStartupAction : IHostStartupAction
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointOutboxHostStartupAction(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public Task Run(CancellationToken token)
        {
            var endpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();

            transport.BindErrorHandler(endpointIdentity, ErrorMessageHandler(endpointIdentity));

            return Task.CompletedTask;
        }

        private Func<IntegrationMessage, CancellationToken, Task> ErrorMessageHandler(EndpointIdentity endpointIdentity)
        {
            return async (message, token) =>
            {
                await using (_dependencyContainer.OpenScopeAsync())
                {
                    var transaction = _dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.Open(true, token).ConfigureAwait(false))
                    {
                        var inbox = await _dependencyContainer
                            .Resolve<IAggregateFactory<Inbox, InboxAggregateSpecification>>()
                            .Build(new InboxAggregateSpecification(message, endpointIdentity), token)
                            .ConfigureAwait(false);

                        if (!inbox.IsError)
                        {
                            inbox.MarkAsError();
                            await transaction.Track(inbox, token).ConfigureAwait(false);
                        }
                    }
                }
            };
        }
    }
}