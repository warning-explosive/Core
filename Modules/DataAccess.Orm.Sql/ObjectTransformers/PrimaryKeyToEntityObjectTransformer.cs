namespace SpaceEngineers.Core.DataAccess.Orm.Sql.ObjectTransformers
{
    using System.Threading;
    using Api.DatabaseEntity;
    using Api.Reading;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class PrimaryKeyToEntityObjectTransformer<TEntity, TKey> : IObjectTransformer<TKey, TEntity>
        where TEntity : IUniqueIdentified<TKey>
    {
        private readonly IReadRepository<TEntity, TKey> _readRepository;

        public PrimaryKeyToEntityObjectTransformer(IReadRepository<TEntity, TKey> readRepository)
        {
            _readRepository = readRepository;
        }

        public TEntity Transform(TKey value)
        {
            return _readRepository.SingleAsync(value, CancellationToken.None).Result;
        }
    }
}