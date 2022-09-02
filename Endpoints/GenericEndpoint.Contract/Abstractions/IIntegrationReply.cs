namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Reply is used for integration between endpoints so as to transfer queried data to requesting endpoint
    ///     - has no one logical owner
    ///     - has multiple receivers (endpoint which requested query)
    ///     - message handler should reply to query initiator endpoint
    /// </summary>
    public interface IIntegrationReply : IIntegrationMessage
    {
    }
}