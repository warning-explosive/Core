namespace SpaceEngineers.Core.SettingsManager
{
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.TypeResolvers;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class YamlFormatter : FileSystemFormatterBase, IYamlFormatter
    {
        private static readonly ISerializer _serializer =
            new SerializerBuilder()
               .WithNamingConvention(PascalCaseNamingConvention.Instance)
               .WithTypeResolver(new DynamicTypeResolver())
               .WithMaximumRecursion(42)
               .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
               .DisableAliases()
               .Build();
        
        private static readonly IDeserializer _deserializer =
            new DeserializerBuilder()
               .WithNamingConvention(PascalCaseNamingConvention.Instance)
               .WithTypeResolver(new DynamicTypeResolver())
               .Build();
        
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