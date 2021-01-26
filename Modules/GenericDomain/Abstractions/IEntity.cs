namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using System;

    /// <summary>
    /// Entity
    /// </summary>
    public interface IEntity : IDomainObject,
                               IUniqueIdentified,
                               IEquatable<IEntity>
    {
    }
}