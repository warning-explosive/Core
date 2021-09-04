namespace SpaceEngineers.Core.DataAccess.Orm.ObjectTransformers
{
    using System;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

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