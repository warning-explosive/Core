namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;

    /// <summary>
    /// Unique identified domain object
    /// </summary>
    public interface IUniqueIdentified
    {
        /// <summary>
        /// Identifier
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Version
        /// </summary>
        ulong Version { get; }
    }
}