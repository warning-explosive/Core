namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using Basics;

    /// <summary>
    /// Entity
    /// </summary>
    public interface IEntity : IDomainObject,
                               IUniqueIdentified,
                               IEquatable<IEntity>,
                               ISafelyEquatable<IEntity>
    {
    }
}