namespace SpaceEngineers.Core.DataImport.Internals
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class DataImporter<TElement, TSpec> : IDataImporter<TElement, TSpec>
        where TSpec : IDataExtractorSpecification
    {
        private readonly IDataExtractor<TSpec> _dataExtractor;

        /// <summary> .cctor </summary>
        /// <param name="dataExtractor">Data extractor</param>
        public DataImporter(IDataExtractor<TSpec> dataExtractor)
        {
            _dataExtractor = dataExtractor;
        }

        /// <inheritdoc />
        public IEnumerable<TElement> Import(
            TSpec specification,
            IDataTableReader<TElement> dataTableReader)
        {
            var dataTable = _dataExtractor.ExtractData(specification, dataTableReader.PropertyToColumnCaption);

            var propertyToColumn = MergeColumns(dataTableReader, dataTable);

            return ReadTable(dataTable, propertyToColumn, dataTableReader);
        }

        private static IReadOnlyDictionary<string, string> MergeColumns(
            IDataTableReader<TElement> dataTableReader,
            DataTable dataTable)
        {
            return dataTableReader.PropertyToColumnCaption
                .Join(dataTable.Columns.OfType<DataColumn>(),
                    col => col.Value,
                    col => col.Caption,
                    (p, dc) =>
                    {
                        var propertyName = p.Key;
                        var dataColumnName = dc.ColumnName;
                        return (propertyName, dataColumnName);
                    })
                .ToDictionary(column => column.propertyName,
                    column => column.dataColumnName);
        }

        private static IEnumerable<TElement> ReadTable(
            DataTable dataTable,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IDataTableReader<TElement> dataTableReader)
        {
            for (var i = 0; i < dataTable.Rows.Count; ++i)
            {
                var row = dataTable.Rows[i];

                yield return dataTableReader.ReadRow(row, propertyToColumn);
            }
        }
    }
}