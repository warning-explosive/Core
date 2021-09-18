namespace SpaceEngineers.Core.DataAccess.Api
{
    using System;
    using Abstractions;

    /// <summary>
    /// Json object
    /// </summary>
    public class JsonObject : IInlinedObject
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        public JsonObject(string value, Type type)
        {
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }
    }
}