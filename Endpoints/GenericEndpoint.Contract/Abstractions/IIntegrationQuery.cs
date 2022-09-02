namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Query is used for integration between endpoints so as to request data from another endpoint
    ///     - can be requested (queried or sent)
    ///     - has one logical owner (endpoint which can handle this query)
    ///     - has multiple senders
    ///     - message handler should reply to query initiator endpoint
    /// </summary>
    /// <typeparam name="TReply">TReply type-argument</typeparam>
    public interface IIntegrationQuery<TReply> : IIntegrationMessage
        where TReply : IIntegrationReply
    {
    }
}