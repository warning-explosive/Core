namespace SpaceEngineers.Core.GenericEndpoint.Contract.Abstractions
{
    /// <summary>
    /// Event that used for integration between endpoints and describe performed action in the past
    ///     - has one logical owner (event publisher)
    ///     - can be subscribed to and unsubscribed from
    ///     - can be published
    /// </summary>
    public interface IIntegrationEvent : IIntegrationMessage
    {
    }
}