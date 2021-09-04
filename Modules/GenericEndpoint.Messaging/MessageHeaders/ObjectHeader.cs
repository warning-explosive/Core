namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Abstractions;

    internal class ObjectHeader : IIntegrationMessageHeader
    {
        public ObjectHeader(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}