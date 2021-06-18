namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using AutoWiring.Api.Abstractions;
    using Newtonsoft.Json;

    /// <summary>
    /// Seam for custom value reading from json input
    /// </summary>
    public interface IObjectTreeValueReader : IResolvable
    {
        /// <summary> Read </summary>
        /// <param name="reader">JsonReader</param>
        /// <param name="parent">Parent IJsonNode</param>
        /// <returns>
        /// read - success or not
        /// value - ValueNode with parsed value
        /// </returns>
        (bool read, ValueNode value) Read(JsonReader reader, IObjectTreeNode parent);
    }
}