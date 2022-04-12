namespace SpaceEngineers.Core.DataAccess.Orm.Sql.ObjectTransformers
{
    using System.Threading;
    using Api.Model;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.ObjectBuilder;

    [Component(EnLifestyle.Scoped)]
    internal class PrimaryKeyToEntityObjectTransformer<TEntity, TKey> : IObjectTransformer<TKey, TEntity>,
                                                                        IResolvable<IObjectTransformer<TKey, TEntity>>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly IDatabaseContext _databaseContext;

        public PrimaryKeyToEntityObjectTransformer(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public TEntity Transform(TKey value)
        {
            return _databaseContext
                .Read<TEntity, TKey>()
                .SingleAsync(value, CancellationToken.None)
                .Result;
        }
    }
}