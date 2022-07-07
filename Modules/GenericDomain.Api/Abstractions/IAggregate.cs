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
        IEnumerable<IDomainEvent> Events { get; }
    }

    /// <summary>
    /// IAggregate
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public interface IAggregate<TAggregate> : IAggregate
        where TAggregate : class, IAggregate<TAggregate>
    {
    }
}