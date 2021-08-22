namespace SpaceEngineers.Core.DataAccess.Orm.ObjectTransformers
{
    using System;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class GuidToEntityObjectTransformer<TEntity> : IObjectTransformer<Guid, TEntity>
        where TEntity : class, IEntity
    {
        private readonly IReadRepository<TEntity> _readRepository;

        public GuidToEntityObjectTransformer(IReadRepository<TEntity> readRepository)
        {
            _readRepository = readRepository;
        }

        public TEntity Transform(Guid value)
        {
            return _readRepository.Single(value);
        }
    }
}