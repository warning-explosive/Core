namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// IAggregate
    /// </summary>
    public interface IAggregate : IEntity
    {
        /// <summary>
        /// Domain events
        /// </summary>
        IReadOnlyCollection<IDomainEvent> Events { get; }
    }
}