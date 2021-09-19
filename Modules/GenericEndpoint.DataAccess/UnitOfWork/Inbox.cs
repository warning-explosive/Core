namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using GenericDomain.Api.Abstractions;
    using Messaging;

    /// <summary>
    /// Inbox
    /// </summary>
    public class Inbox : BaseAggregate
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Integration message</param>
        public Inbox(IntegrationMessage message)
        {
            Message = message;
            Handled = false;
            IsError = false;
        }

        /// <summary>
        /// Integration message
        /// </summary>
        public IntegrationMessage Message { get; }

        /// <summary>
        /// Has the integration message been handled
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// Has the integration message been put into error queue
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Marks the inbox integration message as handled
        /// </summary>
        public void MarkAsHandled()
        {
            Handled = true;
        }

        /// <summary>
        /// Marks the inbox integration message as handled
        /// </summary>
        public void MarkAsError()
        {
            IsError = true;
        }
    }
}