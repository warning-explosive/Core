namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Data extractor
    /// </summary>
    /// <typeparam name="TElement">Element type-argument</typeparam>
    /// <typeparam name="TSpec">Data extractor specification type-argument</typeparam>
    public interface IDataExtractor<TElement, TSpec>
        where TSpec : IDataExtractorSpecification
    {
        /// <summary>
        /// Extract data
        /// </summary>
        /// <param name="specification">Data extractor specification</param>
        /// <returns>Extracted elements stream</returns>
        IAsyncEnumerable<TElement> ExtractData(TSpec specification);
    }
}