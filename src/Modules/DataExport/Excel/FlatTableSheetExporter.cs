namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DocumentFormat.OpenXml.Spreadsheet;

    [Component(EnLifestyle.Singleton)]
    internal class FlatTableSheetExporter<TRow> : IExcelSheetExporter<FlatTableSheetInfo<TRow>>,
                                                  IResolvable<IExcelSheetExporter<FlatTableSheetInfo<TRow>>>
    {
        public void Fill(
            FlatTableSheetInfo<TRow> sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData)
        {
            var (properties, cellValuesMap) = sheetMetadata.GetProperties(typeof(TRow), Array.Empty<string>());
            InsertFlatHeader(sheetMetadata, sheetData, properties);
            InsertFlatData(sheetMetadata, sheetData, properties, sheetInfo.FlatTable, cellValuesMap);
        }

        private static void InsertFlatData(
            SheetMetadata sheetMetadata,
            SheetData sheetData,
            PropertyInfo[] properties,
            IReadOnlyCollection<TRow> data,
            IReadOnlyDictionary<PropertyInfo, CellValues> cellValuesMap)
        {
            var i = 0;

            foreach (var item in data)
            {
                var row = new Row
                {
                    RowIndex = (uint)(i + 2)
                };

                FillRowCells(
                    sheetMetadata,
                    properties,
                    row,
                    it => it.GetValue(item),
                    it => cellValuesMap[it],
                    0);

                sheetData.AppendChild(row);

                i++;
            }
        }

        private static void InsertFlatHeader(
            SheetMetadata sheetMetadata,
            SheetData sheetData,
            PropertyInfo[] properties)
        {
            var row = new Row
            {
                RowIndex = 1
            };

            FillRowCells(sheetMetadata,
                properties,
                row,
                it => it.Name,
                _ => CellValues.SharedString,
                0);

            sheetData.AppendChild(row);
        }

        private static void FillRowCells(
            SheetMetadata sheetMetadata,
            PropertyInfo[] properties,
            Row row,
            Func<PropertyInfo, object?> valueAccessor,
            Func<PropertyInfo, CellValues> dataTypeAccessor,
            int startFromColumn)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                var value = valueAccessor(property);
                var dataType = dataTypeAccessor(property);

                row.AppendChild(sheetMetadata.GetCellValue(value, dataType, row.RowIndex!, i + startFromColumn));
            }
        }
    }
}