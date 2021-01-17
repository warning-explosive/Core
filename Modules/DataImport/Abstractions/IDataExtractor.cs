namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Data extractor
    /// </summary>
    /// <typeparam name="TElement">Element type-argument</typeparam>
    /// <typeparam name="TSpec">Data extractor specification type-argument</typeparam>
    public interface IDataExtractor<TElement, TSpec> : IResolvable
        where TSpec : IDataExtractorSpecification
    {
        /// <summary>
        /// Extract data
        /// </summary>
        /// <param name="specification">Data extractor specification</param>
        /// <returns>Extracted elements</returns>
        ICollection<TElement> ExtractData(TSpec specification);
    }
}