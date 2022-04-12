namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Json;

    [Component(EnLifestyle.Singleton)]
    internal class JsonSettingsProvider<TSettings> : FileSystemSettingsProviderBase<TSettings>,
                                                     IResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, IJsonSettings
    {
        private readonly IJsonSerializer _jsonSerializer;

        public JsonSettingsProvider(
            IJsonSerializer jsonSerializer,
            ISettingsScopeProvider settingsScopeProvider)
            : base(settingsScopeProvider)
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