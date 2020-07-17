namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.TypeResolvers;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class YamlFormatter<TSettings> : FileSystemFormatterBase<TSettings>
        where TSettings : class, IYamlSettings
    {
        private readonly ISerializer _serializer =
            new SerializerBuilder()
               .WithNamingConvention(PascalCaseNamingConvention.Instance)
               .WithTypeResolver(new DynamicTypeResolver())
               .WithMaximumRecursion(42)
               .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
               .DisableAliases()
               .Build();

        private readonly IDeserializer _deserializer =
            new DeserializerBuilder()
               .WithNamingConvention(PascalCaseNamingConvention.Instance)
               .WithTypeResolver(new DynamicTypeResolver())
               .Build();

        protected override string Extension => "yaml";

        protected override string SerializeInternal(TSettings value)
        {
            return _serializer.Serialize(value);
        }

        protected override TSettings DeserializeInternal(string serialized)
        {
            return _deserializer.Deserialize<TSettings>(serialized);
        }
    }
}