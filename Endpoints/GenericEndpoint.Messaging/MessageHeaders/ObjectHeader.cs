namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    internal class ObjectHeader : IIntegrationMessageHeader
    {
        public ObjectHeader(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Name}] - [{Value}]";
        }
    }
}