namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Event is used for integration between endpoints and describes performed action in the past
    ///     - can be published
    ///     - has one logical owner (event publisher)
    ///     - can be subscribed to and unsubscribed from
    /// </summary>
    public interface IIntegrationEvent : IIntegrationMessage
    {
    }
}