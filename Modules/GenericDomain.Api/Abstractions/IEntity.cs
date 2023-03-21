namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using Basics;

    /// <summary>
    /// IEntity
    /// </summary>
    public interface IEntity : IDomainObject,
                               IEquatable<IEntity>,
                               ISafelyEquatable<IEntity>
    {
        /// <summary>
        /// Identifier
        /// </summary>
        Guid Id { get; }
    }
}