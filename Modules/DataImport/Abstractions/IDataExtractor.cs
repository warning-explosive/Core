namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;
    using System.Data;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Data extractor
    /// </summary>
    /// <typeparam name="TSpec">Data extractor specification type-argument</typeparam>
    public interface IDataExtractor<TSpec> : IResolvable
        where TSpec : IDataExtractorSpecification
    {
        /// <summary>
        /// Extract data
        /// </summary>
        /// <param name="specification">Data extractor specification</param>
        /// <param name="propertyToColumnCaption">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>Extracted DataTable</returns>
        DataTable ExtractData(
            TSpec specification,
            IReadOnlyDictionary<string, string> propertyToColumnCaption);
    }
}