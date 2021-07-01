namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    internal class InheritedEvent : BaseEvent
    {
    }
}