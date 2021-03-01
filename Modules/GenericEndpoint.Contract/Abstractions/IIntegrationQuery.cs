namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Query is used to request data from endpoint
    ///     - can be sent
    ///     - has one logical owner (endpoint which can handle this query)
    ///     - has multiple senders
    ///     - message handler must reply to query initiator endpoint
    /// </summary>
    /// <typeparam name="TReply">TReply type-argument</typeparam>
    public interface IIntegrationQuery<TReply> : IIntegrationMessage
        where TReply : IIntegrationMessage
    {
    }
}