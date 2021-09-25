namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Json object
    /// </summary>

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    public record JsonObject : IInlinedObject
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
        public string Value { get; private init; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; private init; }
    }
}