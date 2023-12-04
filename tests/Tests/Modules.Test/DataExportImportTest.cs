namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
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
                    container.Register<IDataExtractor<DataRow, ExcelDataExtractorSpecification>, DataRowExcelDataExtractor>(EnLifestyle.Transient);
                    container.Register<IDataTableReader<DataRow, ExcelTableMetadata>, DataRowDataTableReader>(EnLifestyle.Transient);

                    container.Register<IDataExtractor<PivotRow, ExcelDataExtractorSpecification>, PivotRowExcelDataExtractor>(EnLifestyle.Transient);
                    container.Register<IDataTableReader<PivotRow, ExcelTableMetadata>, PivotRowDataTableReader>(EnLifestyle.Transient);
                }));

            DependencyContainer = fixture.DependencyContainer(options);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ExcelFlatTableRoundRobinTest()
        {
            var exporter = DependencyContainer.Resolve<IExcelExporter>();
            var importer = DependencyContainer.Resolve<IDataExtractor<DataRow, ExcelDataExtractorSpecification>>();

            var elements = new[]
            {
                new DataRow
                {
                    BooleanField = true,
                    StringField = "SomeString",
                    NullableStringField = "SomeNullableString",
                    IntField = 42
                }
            };

            var sheetName = nameof(ExcelFlatTableRoundRobinTest);

            var infos = new ISheetInfo[]
            {
                new FlatTableSheetInfo<DataRow>(sheetName, elements)
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

                foreach (var row in importedElements)
                {
                    Output.WriteLine(row.ToString());
                }

                Assert.NotEmpty(elements);
                Assert.NotEmpty(importedElements);
                Assert.Equal(elements, importedElements);
            }
        }

        [Fact]
        internal void ExcelPivotTableRoundRobinTest()
        {
            var exporter = DependencyContainer.Resolve<IExcelExporter>();
            var importer = DependencyContainer.Resolve<IDataExtractor<PivotRow, ExcelDataExtractorSpecification>>();

            var elements = new[]
            {
                new DataRow
                {
                    BooleanField = true,
                    StringField = nameof(PivotRow.Value1),
                    NullableStringField = "SomeNullableString",
                    IntField = 42
                },
                new DataRow
                {
                    BooleanField = false,
                    StringField = nameof(PivotRow.Value2),
                    NullableStringField = "SomeNullableString",
                    IntField = 42
                },
                new DataRow
                {
                    BooleanField = false,
                    StringField = nameof(PivotRow.Value2),
                    NullableStringField = "SomeNullableString",
                    IntField = 1
                }
            };

            var expectedElements = new[]
            {
                new PivotRow
                {
                    SubGroup1 = "False",
                    SubGroup2 = string.Empty,
                    Value1 = decimal.Zero,
                    Value2 = 43
                },
                new PivotRow
                {
                    SubGroup1 = string.Empty,
                    SubGroup2 = "1",
                    Value1 = decimal.Zero,
                    Value2 = 1
                },
                new PivotRow
                {
                    SubGroup1 = string.Empty,
                    SubGroup2 = "42",
                    Value1 = decimal.Zero,
                    Value2 = 42
                },
                new PivotRow
                {
                    SubGroup1 = "True",
                    SubGroup2 = string.Empty,
                    Value1 = 42,
                    Value2 = decimal.Zero
                },
                new PivotRow
                {
                    SubGroup1 = string.Empty,
                    SubGroup2 = "42",
                    Value1 = 42,
                    Value2 = decimal.Zero
                }
            };

            var sheetName = nameof(ExcelPivotTableRoundRobinTest);

            var infos = new ISheetInfo[]
            {
                new PivotTableSheetInfo<DataRow>(
                    sheetName,
                    elements,
                    row => row.StringField,
                    new[]
                    {
                        new SubGroupInfo<DataRow>(nameof(PivotRow.SubGroup1), row => row.BooleanField.ToString()),
                        new SubGroupInfo<DataRow>(nameof(PivotRow.SubGroup2), row => row.IntField.ToString())
                    },
                    rows => rows.Sum(row => row.IntField),
                    true)
            };

            using (var stream = exporter.ExportXlsx(infos))
            {
                var specification = new ExcelDataExtractorSpecification(
                    stream,
                    sheetName,
                    new Range(0, 5));

                var importedElements = importer
                   .ExtractData(specification)
                   .ToList();

                foreach (var row in importedElements)
                {
                    Output.WriteLine(row.ToString());
                }

                Assert.NotEmpty(elements);
                Assert.NotEmpty(importedElements);
                Assert.Equal(expectedElements, importedElements);
            }
        }

        [ManuallyRegisteredComponent(nameof(DataExportImportTest))]
        private class DataRowExcelDataExtractor : ExcelDataExtractor<DataRow>
        {
            public DataRowExcelDataExtractor(
                IExcelCellValueExtractor cellValueExtractor,
                IExcelColumnsSelectionBehavior columnsSelectionBehavior,
                IDataTableReader<DataRow, ExcelTableMetadata> dataTableReader)
                : base(cellValueExtractor, columnsSelectionBehavior, dataTableReader)
            {
            }
        }

        [ManuallyRegisteredComponent(nameof(DataExportImportTest))]
        private class DataRowDataTableReader : DataTableReaderBase<DataRow, ExcelTableMetadata>
        {
            public override IReadOnlyDictionary<string, string> PropertyToColumnCaption { get; } =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(DataRow.BooleanField)] = nameof(DataRow.BooleanField),
                    [nameof(DataRow.StringField)] = nameof(DataRow.StringField),
                    [nameof(DataRow.NullableStringField)] = nameof(DataRow.NullableStringField),
                    [nameof(DataRow.IntField)] = nameof(DataRow.IntField)
                };

            public override DataRow? ReadRow(
                System.Data.DataRow row,
                int rowIndex,
                IReadOnlyDictionary<string, string> propertyToColumn,
                ExcelTableMetadata tableMetadata)
            {
                if (RowIsEmpty(row, propertyToColumn))
                {
                    return default;
                }

                var testDataRow = new DataRow
                {
                    BooleanField = ReadRequiredBool(row, nameof(DataRow.BooleanField), propertyToColumn),
                    StringField = ReadRequiredString(row, nameof(DataRow.StringField), propertyToColumn),
                    NullableStringField = ReadString(row, nameof(DataRow.NullableStringField), propertyToColumn),
                    IntField = ReadRequiredInt(row, nameof(DataRow.IntField), propertyToColumn, new IFormatProvider[] { CultureInfo.InvariantCulture })
                };

                return testDataRow;
            }
        }

        private class DataRow : IEquatable<DataRow>,
                                ISafelyEquatable<DataRow>
        {
            public bool BooleanField { get; set; }

            public string StringField { get; set; } = default!;

            public string? NullableStringField { get; set; }

            public int IntField { get; set; }

            #region IEquatable

            public static bool operator ==(DataRow? left, DataRow? right)
            {
                return Equatable.Equals(left, right);
            }

            public static bool operator !=(DataRow? left, DataRow? right)
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

            public bool Equals(DataRow? other)
            {
                return Equatable.Equals(this, other);
            }

            public bool SafeEquals(DataRow other)
            {
                return BooleanField.Equals(other.BooleanField)
                    && string.Equals(StringField, other.StringField, StringComparison.Ordinal)
                    && string.Equals(NullableStringField, other.NullableStringField, StringComparison.Ordinal)
                    && IntField.Equals(other.IntField);
            }

            #endregion

            public override string ToString()
            {
                return this
                    .ToPropertyDictionary()
                    .ToString("; ");
            }
        }

        [ManuallyRegisteredComponent(nameof(DataExportImportTest))]
        private class PivotRowExcelDataExtractor : ExcelDataExtractor<PivotRow>
        {
            public PivotRowExcelDataExtractor(
                IExcelCellValueExtractor cellValueExtractor,
                IExcelColumnsSelectionBehavior columnsSelectionBehavior,
                IDataTableReader<PivotRow, ExcelTableMetadata> dataTableReader)
                : base(cellValueExtractor, columnsSelectionBehavior, dataTableReader)
            {
            }
        }

        [ManuallyRegisteredComponent(nameof(DataExportImportTest))]
        private class PivotRowDataTableReader : DataTableReaderBase<PivotRow, ExcelTableMetadata>
        {
            public override IReadOnlyDictionary<string, string> PropertyToColumnCaption { get; } =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(PivotRow.SubGroup1)] = nameof(PivotRow.SubGroup1),
                    [nameof(PivotRow.SubGroup2)] = nameof(PivotRow.SubGroup2),
                    [nameof(PivotRow.Value1)] = nameof(PivotRow.Value1),
                    [nameof(PivotRow.Value2)] = nameof(PivotRow.Value2)
                };

            public override PivotRow? ReadRow(
                System.Data.DataRow row,
                int rowIndex,
                IReadOnlyDictionary<string, string> propertyToColumn,
                ExcelTableMetadata tableMetadata)
            {
                if (RowIsEmpty(row, propertyToColumn))
                {
                    return default;
                }

                var testDataRow = new PivotRow
                {
                    SubGroup1 = ReadRequiredString(row, nameof(PivotRow.SubGroup1), propertyToColumn),
                    SubGroup2 = ReadRequiredString(row, nameof(PivotRow.SubGroup2), propertyToColumn),
                    Value1 = ReadRequiredDecimal(row, nameof(PivotRow.Value1), propertyToColumn, new IFormatProvider[] { CultureInfo.InvariantCulture }),
                    Value2 = ReadRequiredDecimal(row, nameof(PivotRow.Value2), propertyToColumn, new IFormatProvider[] { CultureInfo.InvariantCulture })
                };

                return testDataRow;
            }
        }

        private class PivotRow : IEquatable<PivotRow>,
                                 ISafelyEquatable<PivotRow>
        {
            public string SubGroup1 { get; set; } = default!;

            public string SubGroup2 { get; set; } = default!;

            public decimal Value1 { get; set; }

            public decimal Value2 { get; set; }

            #region IEquatable

            public static bool operator ==(PivotRow? left, PivotRow? right)
            {
                return Equatable.Equals(left, right);
            }

            public static bool operator !=(PivotRow? left, PivotRow? right)
            {
                return !Equatable.Equals(left, right);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    SubGroup1.GetHashCode(StringComparison.Ordinal),
                    SubGroup2.GetHashCode(StringComparison.Ordinal),
                    Value1,
                    Value2);
            }

            public override bool Equals(object? obj)
            {
                return Equatable.Equals(this, obj);
            }

            public bool Equals(PivotRow? other)
            {
                return Equatable.Equals(this, other);
            }

            public bool SafeEquals(PivotRow other)
            {
                return string.Equals(SubGroup1, other.SubGroup1, StringComparison.Ordinal)
                       && string.Equals(SubGroup2, other.SubGroup2, StringComparison.Ordinal)
                       && Value1.Equals(other.Value1)
                       && Value2.Equals(other.Value2);
            }

            #endregion

            public override string ToString()
            {
                return this
                    .ToPropertyDictionary()
                    .ToString("; ");
            }
        }
    }
}