namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    [Feature(nameof(Test))]
    internal record OpenGenericHandlerCommand : IIntegrationCommand
    {
    }
}