namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    /// <summary>
    /// IDomainEvent
    /// </summary>
    public interface IDomainEvent
    {
    }

    /// <summary>
    /// IDomainEvent
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public interface IDomainEvent<TAggregate> : IDomainEvent
        where TAggregate : class, IAggregate<TAggregate>
    {
    }
}