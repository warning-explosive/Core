namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using DocumentFormat.OpenXml.Spreadsheet;

    [Component(EnLifestyle.Singleton)]
    internal class PivotTableSheetExporter<TRow> : IExcelSheetExporter<PivotTableSheetInfo<TRow>>,
                                                   IResolvable<IExcelSheetExporter<PivotTableSheetInfo<TRow>>>
    {
        public void Fill(
            PivotTableSheetInfo<TRow> sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData)
        {
            var headerRow = new Row
            {
                RowIndex = 1
            };

            sheetData.AppendChild(headerRow);

            var dataRows = new Dictionary<string, object>(StringComparer.Ordinal);

            var aggregateFunc = sheetInfo.AggregateFunc;
            var columnKeyFunc = sheetInfo.ColumnKey;
            var subGroupKeyFunctions = sheetInfo.SubGroups.Values.ToArray();

            var columnIndex = subGroupKeyFunctions.Length;
            var subGroupIndex = 0;
            Cell? headerReferenceChild = null;

            foreach (var (columnKey, columnGroup) in sheetInfo.FlatTable.GroupBy(columnKeyFunc, StringComparer.Ordinal))
            {
                var headerCell = sheetMetadata.GetCellValue(columnKey, CellValues.SharedString, headerRow.RowIndex, columnIndex);
                headerReferenceChild ??= headerCell;
                headerRow.AppendChild(headerCell);

                FillSubGroup(columnGroup, subGroupKeyFunctions, aggregateFunc, columnIndex, subGroupIndex, dataRows, sheetInfo, sheetMetadata, headerRow, headerReferenceChild);

                columnIndex++;
            }

            FillTree(2u, sheetInfo, dataRows, sheetData);
            FillZeros(sheetInfo, sheetMetadata, sheetData, headerRow);

            static void FillSubGroup(
                IEnumerable<TRow> data,
                SubGroupInfo<TRow>[] subGroupKeyFunctions,
                Func<IEnumerable<TRow>, decimal> aggregateFunc,
                int columnIndex,
                int subGroupIndex,
                IDictionary<string, object> dataRows,
                PivotTableSheetInfo<TRow> sheetInfo,
                SheetMetadata sheetMetadata,
                Row headerRow,
                Cell headerReferenceChild)
            {
                if (subGroupIndex >= subGroupKeyFunctions.Length)
                {
                    return;
                }

                var subGroupInfo = subGroupKeyFunctions[subGroupIndex];

                foreach (var (subGroupKey, subGroup) in data.GroupBy(subGroupInfo.KeySelector, StringComparer.Ordinal))
                {
                    Dictionary<string, object> map;

                    if (dataRows.TryGetValue("_sub_" + subGroupKey, out var obj))
                    {
                        map = (Dictionary<string, object>)obj;
                    }
                    else
                    {
                        map = new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            ["_key"] = subGroupInfo.Name
                        };

                        dataRows.Add("_sub_" + subGroupKey, map);
                    }

                    List<Cell> row;

                    if (map.TryGetValue("_index", out obj))
                    {
                        row = (List<Cell>)obj;
                    }
                    else
                    {
                        row = new List<Cell>
                        {
                            sheetMetadata.GetCellValue(subGroupKey, CellValues.SharedString, default, subGroupIndex)
                        };

                        map.Add("_index", row);

                        var cellReference = sheetMetadata.ColumnNameProducer.GetCellReference(subGroupIndex, headerRow.RowIndex.Value);

                        var alreadyHasCell = headerRow
                            .OfType<Cell>()
                            .Any(it => string.CompareOrdinal(it.CellReference.Value, cellReference) == 0);

                        if (!alreadyHasCell)
                        {
                            headerRow.InsertBefore(sheetMetadata.GetCellValue(subGroupInfo.Name, CellValues.SharedString, headerRow.RowIndex.Value, subGroupIndex), headerReferenceChild);
                        }
                    }

                    var value = aggregateFunc(subGroup);

                    if (sheetInfo.ShowAbsoluteNumbers)
                    {
                        value = Math.Abs(value);
                    }

                    row.Add(sheetMetadata.GetCellValue(value, CellValues.Number, default, columnIndex));

                    FillSubGroup(subGroup, subGroupKeyFunctions, aggregateFunc, columnIndex, subGroupIndex + 1, map, sheetInfo, sheetMetadata, headerRow, headerReferenceChild);
                }
            }

            static uint FillTree(
                uint rowIndex,
                PivotTableSheetInfo<TRow> sheetInfo,
                Dictionary<string, object> dataRows,
                SheetData sheetData)
            {
                var subGroupPosition = dataRows.TryGetValue("_key", out var subGroupKeyValue)
                                       && subGroupKeyValue is string subGroupKey
                                       && sheetInfo.SubGroups.TryGetValue(subGroupKey, out var subGroupInfo)
                    ? subGroupInfo.Position
                    : SubGroupPosition.Top;

                var rows = subGroupPosition == SubGroupPosition.Top
                    ? dataRows.OrderBy(it => it.Key)
                    : dataRows.OrderByDescending(it => it.Key);

                foreach (var (key, value) in rows)
                {
                    switch (value)
                    {
                        case string when string.Equals(key, "_key", StringComparison.Ordinal):
                        {
                            break;
                        }

                        case List<Cell> cells when string.Equals(key, "_index", StringComparison.Ordinal):
                        {
                            var row = new Row { RowIndex = rowIndex };

                            sheetData.AppendChild(row);

                            foreach (var cell in cells)
                            {
                                cell.CellReference = cell.CellReference.Value.Replace("0", rowIndex.ToString());
                                row.AppendChild(cell);
                            }

                            rowIndex++;

                            break;
                        }

                        case Dictionary<string, object> map when key.StartsWith("_sub_", StringComparison.Ordinal):
                        {
                            rowIndex = FillTree(rowIndex, sheetInfo, map, sheetData);

                            break;
                        }

                        default:
                        {
                            throw new NotSupportedException($"[{key}] - {value.GetType().FullName}");
                        }
                    }
                }

                return rowIndex;
            }

            static void FillZeros(
                PivotTableSheetInfo<TRow> sheetInfo,
                SheetMetadata sheetMetadata,
                SheetData sheetData,
                Row headerRow)
            {
                var dataColumnsLength = headerRow.Count() - sheetInfo.SubGroups.Count;

                foreach (var row in sheetData.OfType<Row>())
                {
                    for (var index = 0; index < dataColumnsLength; index++)
                    {
                        var columnIndex = sheetInfo.SubGroups.Count + index;

                        var cellReference = sheetMetadata.ColumnNameProducer.GetCellReference(columnIndex, row.RowIndex.Value);

                        var alreadyHasCell = row
                            .OfType<Cell>()
                            .Any(it => string.CompareOrdinal(it.CellReference.Value, cellReference) == 0);

                        if (alreadyHasCell)
                        {
                            continue;
                        }

                        var reference = row
                            .OfType<Cell>()
                            .LastOrDefault(it => string.CompareOrdinal(it.CellReference.Value, cellReference) < 0);

                        var cell = sheetMetadata.GetCellValue(0m, CellValues.Number, row.RowIndex.Value, columnIndex);

                        if (reference != null)
                        {
                            row.InsertAfter(cell, reference);
                        }
                        else
                        {
                            row.AppendChild(cell);
                        }
                    }
                }
            }
        }
    }
}