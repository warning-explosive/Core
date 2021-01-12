namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Data importer
    /// </summary>
    /// <typeparam name="TElement">Element type-argument</typeparam>
    /// <typeparam name="TSpec">Data extractor specification type-argument</typeparam>
    public interface IDataImporter<TElement, TSpec> : IResolvable
        where TSpec : IDataExtractorSpecification
    {
        /// <summary>
        /// Import
        /// </summary>
        /// <param name="specification">Data extractor specification</param>
        /// <param name="dataTableReader">Data table reader</param>
        /// <returns>Elements collections</returns>
        IEnumerable<TElement> Import(
            TSpec specification,
            IDataTableReader<TElement> dataTableReader);
    }
}