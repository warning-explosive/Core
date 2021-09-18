namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
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
                // TODO: #100 - add polling timeout
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
                // TODO: not from error queue
                var subsequentMessages = (await _dependencyContainer
                        .Resolve<IReadRepository<IntegrationMessageDatabaseEntity, Guid>>()
                        .All()
                        .Where(message => !message.Sent)
                        .ToListAsync(token)
                        .ConfigureAwait(false))
                    .Select(message => message.BuildIntegrationMessage(serializer, formatter))
                    .ToList();

                var outbox = new Outbox(new IntegrationMessage(default!, typeof(object), formatter), subsequentMessages);

                await outbox.DeliverMessages(
                        transport,
                        _dependencyContainer.Resolve<IDatabaseTransaction>(),
                        token)
                    .ConfigureAwait(false);
            }
        }
    }
}