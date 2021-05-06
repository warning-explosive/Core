namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class BulkRepository<TAggregate> : IBulkRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        public Task Create(IEnumerable<TAggregate> aggregate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update(IEnumerable<TAggregate> aggregate, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(IEnumerable<TAggregate> aggregate, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}