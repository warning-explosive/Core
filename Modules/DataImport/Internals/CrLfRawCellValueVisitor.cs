namespace SpaceEngineers.Core.DataImport.Internals
{
    using System;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    /// <summary>
    /// CrLfRawCellValueVisitor
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public class CrLfRawCellValueVisitor : IRawCellValueVisitor,
                                           ICollectionResolvable<IRawCellValueVisitor>
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