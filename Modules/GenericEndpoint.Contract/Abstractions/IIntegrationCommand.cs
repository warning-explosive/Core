namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Command is used for integration between endpoints so as to perform integration action and state change
    ///     - can be sent
    ///     - has one logical owner (endpoint which can handle this command)
    ///     - has multiple senders
    ///     - only commands can introduce changes in the database (message handlers should send commands for that purpose)
    /// </summary>
    public interface IIntegrationCommand : IIntegrationMessage
    {
    }
}