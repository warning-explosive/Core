namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class JsonSettingsProvider<TSettings> : FileSystemSettingsProviderBase<TSettings>
        where TSettings : class, IJsonSettings
    {
        private readonly IJsonSerializer _jsonSerializer;

        public JsonSettingsProvider(IJsonSerializer jsonSerializer)
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