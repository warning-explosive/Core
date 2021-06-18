namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Abstractions
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