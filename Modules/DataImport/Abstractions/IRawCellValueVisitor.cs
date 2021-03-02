namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// Raw cell value visitor
    /// </summary>
    public interface IRawCellValueVisitor : ICollectionResolvable<IRawCellValueVisitor>
    {
        /// <summary>
        /// Visit cell value
        /// </summary>
        /// <param name="value">Previous value</param>
        /// <returns>Modified value</returns>
        string Visit(string value);
    }
}