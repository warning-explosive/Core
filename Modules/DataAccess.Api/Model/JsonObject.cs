namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Json object
    /// </summary>

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    public record JsonObject : IInlinedObject
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        /// <param name="systemType">System type</param>
        public JsonObject(string value, SystemType systemType)
        {
            Value = value;
            SystemType = systemType;
        }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// System type
        /// </summary>
        public SystemType SystemType { get; init; }
    }
}