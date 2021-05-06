namespace SpaceEngineers.Core.GenericDomain.Abstractions
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