namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IDomainTypeProvider
    /// </summary>
    public interface IDomainTypeProvider : IResolvable
    {
        /// <summary>
        /// Gets domain entities
        /// </summary>
        /// <returns>Domain entities</returns>
        IEnumerable<Type> Entities();
    }
}