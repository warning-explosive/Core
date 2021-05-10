namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class EntityToGuidObjectTransformer<TEntity> : IObjectTransformer<TEntity, Guid>
        where TEntity : class, IEntity
    {
        public Guid Transform(TEntity value)
        {
            return value.Id;
        }
    }
}