namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Reply is used for integration between endpoints so as to transfer queried data to requesting endpoint
    ///     - has no one logical owner
    ///     - has multiple receivers (endpoints that performed requests)
    ///     - message handler should provide a reply in response on incoming request
    /// </summary>
    public interface IIntegrationReply : IIntegrationMessage
    {
    }
}