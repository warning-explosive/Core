namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract.Abstractions;
    using Contract.Attributes;
    using Core.DataAccess.Api.Abstractions;
    using Core.DataAccess.Api.Extensions;
    using CrossCuttingConcerns.Api.Abstractions;
    using DatabaseModel;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using UnitOfWork;

    internal class GenericEndpointOutboxHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointOutboxHostBackgroundWorker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // TODO: #154 - add polling timeout
                await Task.Delay(TimeSpan.FromSeconds(42), token).ConfigureAwait(false);
                await DeliverMessages(token).ConfigureAwait(false);
            }
        }

        private Task DeliverMessages(CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(DeliverMessagesUnsafe)
                .Catch<Exception>()
                .Invoke(token);
        }

        private async Task DeliverMessagesUnsafe(CancellationToken token)
        {
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();
            var serializer = _dependencyContainer.Resolve<IJsonSerializer>();
            var formatter = _dependencyContainer.Resolve<IStringFormatter>();

            await using (_dependencyContainer.OpenScopeAsync())
            {
                var transaction = _dependencyContainer.Resolve<IDatabaseTransaction>();

                await using (await transaction.Open(true, token).ConfigureAwait(false))
                {
                    var subsequentMessages = (await _dependencyContainer
                            .Resolve<IReadRepository<IntegrationMessageDatabaseEntity, Guid>>()
                            .All()
                            .Where(message => !message.Sent && !message.Handled && !message.IsError)
                            .ToListAsync(token)
                            .ConfigureAwait(false))
                        .Select(message => message.BuildIntegrationMessage(serializer, formatter))
                        .ToList();

                    var fakeInitiator = new IntegrationMessage(new DeliverOutboxMessages(), typeof(DeliverOutboxMessages), formatter);
                    var outbox = new Outbox(fakeInitiator, subsequentMessages);

                    await outbox.DeliverMessages(
                            transport,
                            transaction,
                            token)
                        .ConfigureAwait(false);
                }
            }
        }

        [OwnedBy(nameof(Outbox))]
        private class DeliverOutboxMessages : IIntegrationCommand
        {
        }
    }
}