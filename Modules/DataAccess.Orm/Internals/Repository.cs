namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TAggregate> : IRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        public Task Create(TAggregate aggregate)
        {
            throw new NotImplementedException();
        }

        public Task Update(TAggregate aggregate)
        {
            throw new NotImplementedException();
        }

        public Task Delete(TAggregate aggregate)
        {
            throw new NotImplementedException();
        }
    }
}