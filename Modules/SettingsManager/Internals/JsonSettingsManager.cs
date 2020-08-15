namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Newtonsoft.Json;
    using NewtonSoft.Json.Abstractions;

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