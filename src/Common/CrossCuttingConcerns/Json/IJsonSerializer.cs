namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;

    /// <summary>
    /// IJsonSerializer abstraction
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="value">Object for serialization</param>
        /// <param name="type">Object type</param>
        /// <returns>Serialized object</returns>
        string SerializeObject(object value, Type type);

        /// <summary>
        /// Deserialize object
        /// </summary>
        /// <param name="serialized">Serialized object</param>
        /// <param name="type">Target type</param>
        /// <returns>Deserialized object</returns>
        object DeserializeObject(string serialized, Type type);

        /// <summary>
        /// Deserialize object (typed version)
        /// </summary>
        /// <param name="serialized">Serialized object</param>
        /// <typeparam name="T">Target type type-argument</typeparam>
        /// <returns>Deserialized object</returns>
        T DeserializeObject<T>(string serialized);
    }
}