namespace SpaceEngineers.Core.StatisticsEndpoint.Model
{
    using System;
    using GenericDomain;

    internal class FailedMessage : EntityBase
    {
        public FailedMessage(MessageInfo message, Exception exception)
        {
            Message = message;
            Exception = exception.ToString();
        }

        public MessageInfo Message { get; }

        public string Exception { get; }
    }
}