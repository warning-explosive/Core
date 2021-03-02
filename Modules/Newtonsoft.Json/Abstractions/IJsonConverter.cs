namespace SpaceEngineers.Core.NewtonSoft.Json.Abstractions
{
    using AutoWiring.Api.Abstractions;
    using Newtonsoft.Json;

    /// <summary>
    /// IJsonConverter abstraction
    /// </summary>
    public interface IJsonConverter : ICollectionResolvable<IJsonConverter>
    {
        /// <summary>
        /// JsonConverter
        /// </summary>
        JsonConverter Converter { get; }
    }
}