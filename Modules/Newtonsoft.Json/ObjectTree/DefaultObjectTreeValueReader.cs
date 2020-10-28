namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Newtonsoft.Json;

    /// <summary>
    /// DefaultObjectTreeValueReader
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    public sealed class DefaultObjectTreeValueReader : IObjectTreeValueReader
    {
        /// <inheritdoc />
        public (bool read, ValueNode value) Read(JsonReader reader, IObjectTreeNode parent)
        {
            var value = new ValueNode(parent, reader.Value);
            return (reader.Read(), value);
        }
    }
}