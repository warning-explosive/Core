namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class EventStore : IEventStore,
                                IResolvable<IEventStore>
    {
        public Task<TAggregate?> Get<TAggregate>(Guid id, DateTime utcNow)
            where TAggregate : class, IAggregate<TAggregate>
        {
            // TODO: #172 - implement reading/appending/snapshots
            return Task.FromResult<TAggregate?>(default);
        }
    }
}