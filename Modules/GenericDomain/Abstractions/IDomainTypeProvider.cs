namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IDomainTypeProvider
    /// </summary>
    public interface IDomainTypeProvider : IResolvable
    {
        /// <summary>
        /// Gets domain entities (including aggregates)
        /// </summary>
        /// <returns>Domain entities</returns>
        IEnumerable<Type> Entities();

        /// <summary>
        /// Gets domain aggregates (excluding entities)
        /// </summary>
        /// <returns>Domain aggregates</returns>
        IEnumerable<Type> Aggregates();

        /// <summary>
        /// Gets domain value objects
        /// </summary>
        /// <returns>Domain value objects</returns>
        IEnumerable<Type> ValueObjects();

        /// <summary>
        /// Gets domain enumeration objects
        /// </summary>
        /// <returns>Domain enumeration objects</returns>
        IEnumerable<Type> EnumerationObjects();
    }
}