namespace SpaceEngineers.Core.DataAccess.Orm.Sql.ObjectTransformers
{
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class EntityToPrimaryKeyObjectTransformer<TEntity, TKey> : IObjectTransformer<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
    {
        public TKey Transform(TEntity value)
        {
            return value.PrimaryKey;
        }
    }
}