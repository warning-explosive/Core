namespace SpaceEngineers.Core.Utilities.SettingsManager
{
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using YamlDotNet.Serialization;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class YamlFormatter : FileSystemFormatterBase, IYamlFormatter
    {
        private static readonly ISerializer _serializer = new SerializerBuilder().Build();

        private static readonly IDeserializer _deserializer = new DeserializerBuilder().Build();
        
        protected override string Extension => "yaml";
        
        protected override string SerializeInternal<TSettings>(TSettings value)
        {
            return _serializer.Serialize(value);
        }

        protected override TSettings DeserializeInternal<TSettings>(string serialized)
        {
            return _deserializer.Deserialize<TSettings>(serialized);
        }
    }
}