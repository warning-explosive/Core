namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.ObjectTransformers
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.ObjectBuilder;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class EntityToPrimaryKeyObjectTransformer<TEntity, TKey> : IObjectTransformer<TEntity, TKey>,
                                                                        IResolvable<IObjectTransformer<TEntity, TKey>>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        public TKey Transform(TEntity value)
        {
            return value.PrimaryKey;
        }
    }
}