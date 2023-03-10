namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.ObjectTransformers
{
    using System.Threading;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.ObjectBuilder;
    using Model;
    using Transaction;

    [Component(EnLifestyle.Scoped)]
    internal class PrimaryKeyToEntityObjectTransformer<TEntity, TKey> : IObjectTransformer<TKey, TEntity>,
                                                                        IResolvable<IObjectTransformer<TKey, TEntity>>
        where TEntity : IDatabaseEntity<TKey>
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
                .Single<TEntity, TKey>(value, CancellationToken.None)
                .Result;
        }
    }
}