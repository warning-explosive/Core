namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    /// <summary>
    /// Query is used to request data from endpoint
    ///     - can be sent
    ///     - has one logical owner (endpoint which can handle this query)
    ///     - has multiple senders
    ///     - message handler must reply to query initiator endpoint // TODO: validate
    /// </summary>
    /// <typeparam name="TResponse">TResponse type-argument</typeparam>
    public interface IIntegrationQuery<TResponse> : IIntegrationMessage
        where TResponse : IIntegrationMessage
    {
    }
}