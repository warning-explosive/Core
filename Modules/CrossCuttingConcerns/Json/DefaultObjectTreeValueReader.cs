namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Newtonsoft.Json;

    /// <summary>
    /// DefaultObjectTreeValueReader
    /// </summary>
    [Component(EnLifestyle.Singleton)]
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