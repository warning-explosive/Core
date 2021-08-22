namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.TypeResolvers;

    [Component(EnLifestyle.Singleton)]
    internal class YamlSettingsProvider<TSettings> : FileSystemSettingsProviderBase<TSettings>
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