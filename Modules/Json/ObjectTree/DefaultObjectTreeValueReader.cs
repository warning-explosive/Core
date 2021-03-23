namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
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
            var value = new ValueNode(parent, reader.Path, reader.Value);
            return (reader.Read(), value);
        }
    }
}