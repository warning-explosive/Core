namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataExport.Excel;
    using DataImport;
    using DataImport.Abstractions;
    using DataImport.Excel;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataExport/DataImport assembly tests
    /// </summary>
    public class DataExportImportTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public DataExportImportTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DataExport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DataImport)))
            };

            var options = new DependencyContainerOptions()
                .WithPluginAssemblies(assemblies)
                .WithManualRegistrations(fixture.DelegateRegistration(container =>
                {
                    container.Register<IDataExtractor<TestDataRow, ExcelDataExtractorSpecification>, TestDataRowExcelDataExtractor>(EnLifestyle.Transient);
                    container.Register<IDataTableReader<TestDataRow, ExcelTableMetadata>, TestDataRowDataTableReader>(EnLifestyle.Transient);
                }));

            DependencyContainer = fixture.DependencyContainer(options);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ExcelRoundRobinTest()
        {
            var exporter = DependencyContainer.Resolve<IExcelExporter>();
            var importer = DependencyContainer.Resolve<IDataExtractor<TestDataRow, ExcelDataExtractorSpecification>>();

            var elements = new[]
            {
                new TestDataRow
                {
                    BooleanField = true,
                    StringField = "SomeString",
                    NullableStringField = "SomeNullableString",
                    IntField = 42
                }
            };

            var sheetName = nameof(ExcelRoundRobinTest);

            var infos = new ISheetInfo[]
            {
                new FlatTableSheetInfo<TestDataRow>(elements) { SheetName = sheetName }
            };

            using (var stream = exporter.ExportXlsx(infos))
            {
                var specification = new ExcelDataExtractorSpecification(
                    stream,
                    sheetName,
                    new Range(0, 1));

                var importedElements = importer
                   .ExtractData(specification)
                   .ToList();

                Assert.NotEmpty(elements);
                Assert.NotEmpty(importedElements);
                Assert.Equal(elements, importedElements);
            }
        }

        [ManuallyRegisteredComponent(nameof(DataExportImportTest))]
        private class TestDataRowExcelDataExtractor : ExcelDataExtractor<TestDataRow>
        {
            public TestDataRowExcelDataExtractor(
                IExcelCellValueExtractor cellValueExtractor,
                IExcelColumnsSelectionBehavior columnsSelectionBehavior,
                IDataTableReader<TestDataRow, ExcelTableMetadata> dataTableReader)
                : base(cellValueExtractor, columnsSelectionBehavior, dataTableReader)
            {
            }
        }

        [ManuallyRegisteredComponent(nameof(DataExportImportTest))]
        private class TestDataRowDataTableReader : DataTableReaderBase<TestDataRow, ExcelTableMetadata>
        {
            public override IReadOnlyDictionary<string, string> PropertyToColumnCaption { get; } =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(TestDataRow.BooleanField)] = nameof(TestDataRow.BooleanField),
                    [nameof(TestDataRow.StringField)] = nameof(TestDataRow.StringField),
                    [nameof(TestDataRow.NullableStringField)] = nameof(TestDataRow.NullableStringField),
                    [nameof(TestDataRow.IntField)] = nameof(TestDataRow.IntField)
                };

            public override TestDataRow? ReadRow(
                DataRow row,
                int rowIndex,
                IReadOnlyDictionary<string, string> propertyToColumn,
                ExcelTableMetadata tableMetadata)
            {
                if (RowIsEmpty(row, propertyToColumn))
                {
                    return default;
                }

                var testDataRow = new TestDataRow
                {
                    BooleanField = ReadRequiredBool(row, nameof(TestDataRow.BooleanField), propertyToColumn),
                    StringField = ReadRequiredString(row, nameof(TestDataRow.StringField), propertyToColumn),
                    NullableStringField = ReadString(row, nameof(TestDataRow.NullableStringField), propertyToColumn),
                    IntField = ReadRequiredInt(row, nameof(TestDataRow.IntField), propertyToColumn, new IFormatProvider[] { CultureInfo.InvariantCulture })
                };

                return testDataRow;
            }
        }

        private class TestDataRow : IEquatable<TestDataRow>,
                                    ISafelyEquatable<TestDataRow>
        {
            public bool BooleanField { get; set; }

            public string StringField { get; set; } = default!;

            public string? NullableStringField { get; set; }

            public int IntField { get; set; }

            #region IEquatable

            public static bool operator ==(TestDataRow? left, TestDataRow? right)
            {
                return Equatable.Equals(left, right);
            }

            public static bool operator !=(TestDataRow? left, TestDataRow? right)
            {
                return !Equatable.Equals(left, right);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    BooleanField,
                    StringField.GetHashCode(StringComparison.Ordinal),
                    NullableStringField.GetHashCode(StringComparison.Ordinal),
                    IntField);
            }

            public override bool Equals(object? obj)
            {
                return Equatable.Equals(this, obj);
            }

            public bool Equals(TestDataRow? other)
            {
                return Equatable.Equals(this, other);
            }

            public bool SafeEquals(TestDataRow other)
            {
                return BooleanField.Equals(other.BooleanField)
                    && string.Equals(StringField, other.StringField, StringComparison.Ordinal)
                    && string.Equals(NullableStringField, other.NullableStringField, StringComparison.Ordinal)
                    && IntField.Equals(other.IntField);
            }

            #endregion
        }
    }
}