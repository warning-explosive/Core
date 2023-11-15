namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Request is used for integration between endpoints so as to request data from another endpoint
    ///     - can be requested (queried or sent)
    ///     - has one logical owner (endpoint which can handle this request)
    ///     - has multiple senders
    ///     - message handler should provide a reply in response on incoming request
    /// </summary>
    /// <typeparam name="TReply">TReply type-argument</typeparam>
    public interface IIntegrationRequest<TReply> : IIntegrationMessage
        where TReply : IIntegrationReply
    {
    }
}