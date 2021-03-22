namespace SpaceEngineers.Core.DataImport
{
    using System;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

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