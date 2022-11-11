namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;

    /// <summary>
    /// DomainEventArgs
    /// </summary>
    public class DomainEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="details">DomainEventDetails</param>
        /// <param name="domainEvent">IDomainEvent</param>
        public DomainEventArgs(
            DomainEventDetails details,
            IDomainEvent domainEvent)
        {
            Details = details;
            DomainEvent = domainEvent;
        }

        /// <summary>
        /// Details
        /// </summary>
        public DomainEventDetails Details { get; init; }

        /// <summary>
        /// DomainEvent
        /// </summary>
        public IDomainEvent DomainEvent { get; init; }
    }
}