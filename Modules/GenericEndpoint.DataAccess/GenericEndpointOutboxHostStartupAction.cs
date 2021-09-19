namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract;
    using Core.DataAccess.Api.Abstractions;
    using GenericDomain.Api.Abstractions;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using UnitOfWork;

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

            transport.BindErrorHandler(endpointIdentity, ErrorMessageHandler(endpointIdentity, token));

            return Task.CompletedTask;
        }

        private Func<IntegrationMessage, Task> ErrorMessageHandler(EndpointIdentity endpointIdentity, CancellationToken token)
        {
            return async message =>
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
                            await transaction.Upsert(inbox, token).ConfigureAwait(false);
                        }
                    }
                }
            };
        }
    }
}