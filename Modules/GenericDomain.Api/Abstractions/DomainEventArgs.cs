namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;

    /// <summary>
    /// DomainEventArgs
    /// </summary>
    public class DomainEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="domainEvent">IDomainEvent</param>
        public DomainEventArgs(IDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }

        /// <summary>
        /// IDomainEvent
        /// </summary>
        public IDomainEvent DomainEvent { get; }
    }
}