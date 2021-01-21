namespace SpaceEngineers.Core.DataImport.Internals
{
    using System;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    public class CrLfRawCellValueVisitor : IRawCellValueVisitor
    {
        /// <inheritdoc />
        public string Visit(string value)
        {
            return value
                .Replace("\r", string.Empty, StringComparison.Ordinal)
                .Replace("\n", string.Empty, StringComparison.Ordinal);
        }
    }
}