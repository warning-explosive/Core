namespace SpaceEngineers.Core.NewtonSoft.Json.Abstractions
{
    using AutoWiringApi.Abstractions;
    using Newtonsoft.Json;

    /// <summary>
    /// IJsonConverter abstraction
    /// </summary>
    public interface IJsonConverter : ICollectionResolvable
    {
        /// <summary>
        /// JsonConverter
        /// </summary>
        JsonConverter Converter { get; }
    }
}