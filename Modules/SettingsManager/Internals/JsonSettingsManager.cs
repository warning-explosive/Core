namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Json.Abstractions;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class JsonSettingsManager<TSettings> : FileSystemSettingsManagerBase<TSettings>
        where TSettings : class, IJsonSettings
    {
        private readonly IJsonSerializer _jsonSerializer;

        public JsonSettingsManager(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        protected override string Extension => "json";

        protected override string SerializeInternal(TSettings value)
        {
            return _jsonSerializer.SerializeObject(value);
        }

        protected override TSettings DeserializeInternal(string serialized)
        {
            return _jsonSerializer.DeserializeObject<TSettings>(serialized);
        }
    }
}