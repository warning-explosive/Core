namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    [Feature(TestFeatures.Test)]
    internal record InheritedEvent : BaseEvent
    {
        public InheritedEvent(int id)
            : base(id)
        {
        }
    }
}