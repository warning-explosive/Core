namespace SpaceEngineers.Core.AuthorizationEndpoint.Contract.Messages
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// Create user command
    /// </summary>
    [OwnedBy(AuthorizationEndpointIdentity.LogicalName)]
    public class CreateUser : IIntegrationCommand
    {
    }
}