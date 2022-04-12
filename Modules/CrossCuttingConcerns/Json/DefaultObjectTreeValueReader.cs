namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Newtonsoft.Json;

    [Component(EnLifestyle.Singleton)]
    internal sealed class DefaultObjectTreeValueReader : IObjectTreeValueReader,
                                                         IResolvable<IObjectTreeValueReader>
    {
        public (bool read, ValueNode value) Read(JsonReader reader, IObjectTreeNode parent)
        {
            var value = new ValueNode(parent, reader.Path, reader.Value);
            return (reader.Read(), value);
        }
    }
}