namespace SpaceEngineers.Core.DataImport.Internals
{
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
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
        }
    }
}