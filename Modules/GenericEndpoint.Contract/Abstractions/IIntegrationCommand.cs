namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Command that used to make request to perform integration action between endpoints
    ///     - can be sent
    ///     - has one logical owner (endpoint which can handle this command)
    ///     - has multiple senders
    /// </summary>
    public interface IIntegrationCommand : IIntegrationMessage
    {
    }
}