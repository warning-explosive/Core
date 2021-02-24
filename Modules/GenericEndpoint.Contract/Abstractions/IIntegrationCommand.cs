namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Command that used to make request to perform integration action between endpoints
    ///     - can be sent
    ///     - has one logical owner (endpoint which can handle this command)
    ///     - has multiple senders
    ///     - TODO: only commands can introduce changes in application storage (another kinds of message must send command for that purpose)
    /// </summary>
    public interface IIntegrationCommand : IIntegrationMessage
    {
    }
}