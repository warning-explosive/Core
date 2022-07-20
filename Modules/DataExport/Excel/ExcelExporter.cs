namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    [Component(EnLifestyle.Singleton)]
    internal class ExcelExporter : IExcelExporter,
                                   IResolvable<IExcelExporter>
    {
        private const string DateTimeFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        public MemoryStream ExportXlsx(ISheetInfo[] infos)
        {
            var stream = new MemoryStream();

            try
            {
                FillStream(infos, stream);

                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
        }

        private static void FillStream(ISheetInfo[] infos, MemoryStream stream)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                var sharedStringTable = workbookPart.AddNewPart<SharedStringTablePart>();
                sharedStringTable.SharedStringTable = new SharedStringTable();

                var workbookStyles = workbookPart.AddNewPart<WorkbookStylesPart>();
                workbookStyles.Stylesheet = InitializeStylesheet(
                    out var defaultCellFormatIndex,
                    out var decimalCellFormatIndex,
                    out var dateCellFormatIndex);

                var documentInfo = new DocumentInfo(
                    sharedStringTable.SharedStringTable,
                    workbookStyles.Stylesheet,
                    defaultCellFormatIndex,
                    decimalCellFormatIndex,
                    dateCellFormatIndex);

                for (uint i = 0; i < infos.Length; i++)
                {
                    var info = infos[i];
                    var sheetInfo = new SheetInfo(documentInfo);

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheet = new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = i + 1,
                        Name = info.SheetName
                    };

                    sheets.AppendChild(sheet);

                    var type = info.GetType();

                    if (type.IsSubclassOfOpenGeneric(typeof(FlatTableSheetInfo<>)))
                    {
                        FillFlatTable(sheetInfo, info, sheetData);
                    }
                    else
                    {
                        throw new NotSupportedException(type.FullName);
                    }
                }
            }
        }

        private static Stylesheet InitializeStylesheet(
            out uint defaultCellFormatIndex,
            out uint decimalCellFormatIndex,
            out uint dateCellFormatIndex)
        {
            var stylesheet = new Stylesheet();

            var fonts = new Fonts
            {
                Count = 0,
                KnownFonts = BooleanValue.FromBoolean(true)
            };

            var font = new Font
            {
                FontSize = new FontSize { Val = 11 },
                FontName = new FontName { Val = "Calibri" },
                Color = new Color { Theme = 1 },
                FontFamilyNumbering = new FontFamilyNumbering { Val = 2 },
                FontScheme = new FontScheme { Val = new EnumValue<FontSchemeValues>(FontSchemeValues.Minor) }
            };

            fonts.Append(font);
            fonts.Count++;

            stylesheet.Append(fonts);

            var fills = new Fills
            {
                Count = 0
            };

            var fill = new Fill
            {
                PatternFill = new PatternFill
                {
                    PatternType = new EnumValue<PatternValues>(PatternValues.None)
                }
            };

            fills.Append(fill);
            fills.Count++;

            stylesheet.Append(fills);

            var borders = new Borders
            {
                Count = 0
            };

            var border = new Border
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            };

            borders.Append(border);
            borders.Count++;

            stylesheet.Append(borders);

            var defaultCellFormat = new CellFormat
            {
                NumberFormatId = 0,
                FormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0
            };

            var decimalCellFormat = new CellFormat
            {
                NumberFormatId = 2U,
                FormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyNumberFormat = true
            };

            var dateCellFormat = new CellFormat
            {
                NumberFormatId = 14U,
                FormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyNumberFormat = true
            };

            var cellFormats = new CellFormats
            {
                Count = 0
            };

            // 0
            cellFormats.Append(defaultCellFormat);
            cellFormats.Count++;
            defaultCellFormatIndex = 0;

            // 1
            cellFormats.Append(decimalCellFormat);
            cellFormats.Count++;
            decimalCellFormatIndex = 1;

            // 2
            cellFormats.Append(dateCellFormat);
            cellFormats.Count++;
            dateCellFormatIndex = 2;

            stylesheet.Append(cellFormats);

            return stylesheet;
        }

        private static void FillFlatTable(
            SheetInfo sheetInfo,
            ISheetInfo info,
            SheetData sheetData)
        {
            _ = typeof(ExcelExporter)
               .CallMethod(nameof(FillFlatTableGeneric))
               .WithTypeArguments(info.GetType().GetGenericArguments())
               .WithArguments(sheetInfo, info, sheetData)
               .Invoke();
        }

        private static void FillFlatTableGeneric<TRow>(
            SheetInfo sheetInfo,
            FlatTableSheetInfo<TRow> flatTableSheetInfo,
            SheetData sheetData)
        {
            var (properties, cellValuesMap) = GetProperties(typeof(TRow), Array.Empty<string>());
            InsertFlatHeader(sheetInfo, sheetData, properties);
            InsertData(sheetInfo, sheetData, properties, flatTableSheetInfo.FlatTable, cellValuesMap);
        }

        private static (PropertyInfo[], IReadOnlyDictionary<PropertyInfo, CellValues>) GetProperties(
            Type type,
            IReadOnlyCollection<string> columnsOrder)
        {
            var columnsPriorities = columnsOrder
               .Select((value, index) => (value, index))
               .ToDictionary(
                    it => it.value,
                    it => it.index,
                    StringComparer.OrdinalIgnoreCase);

            var properties = type
               .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
               .Where(property => property.GetIsAccessible())
               .Where(property => !property.PropertyType.IsCollection() || typeof(string) == property.PropertyType)
               .OrderBy(property => columnsPriorities.TryGetValue(GetPropertyName(property), out var priority)
                    ? priority
                    : int.MaxValue)
               .ThenBy(GetPropertyName)
               .ToArray();

            var cellValuesMap = properties
               .ToDictionary(
                    it => it,
                    it => GetDataType(it.PropertyType));

            return (properties, cellValuesMap);
        }

        private static void InsertData<T>(
            SheetInfo sheetInfo,
            SheetData sheetData,
            PropertyInfo[] properties,
            IReadOnlyCollection<T> data,
            IReadOnlyDictionary<PropertyInfo, CellValues> cellValuesMap)
        {
            data.Each((item, i) =>
            {
                var row = new Row
                {
                    RowIndex = (uint)(i + 2)
                };

                FillRowCells(sheetInfo,
                    properties,
                    row,
                    it => ConvertValue(it.GetValue(item)),
                    it => cellValuesMap[it],
                    0);

                sheetData.AppendChild(row);
            });
        }

        private static void InsertFlatHeader(
            SheetInfo sheetInfo,
            SheetData sheetData,
            PropertyInfo[] properties)
        {
            var row = new Row
            {
                RowIndex = 1
            };

            FillRowCells(sheetInfo,
                properties,
                row,
                GetPropertyName,
                _ => CellValues.SharedString,
                0);

            sheetData.AppendChild(row);
        }

        private static void FillRowCells(
            SheetInfo sheetInfo,
            PropertyInfo[] properties,
            Row row,
            Func<PropertyInfo, string> valueAccessor,
            Func<PropertyInfo, CellValues> dataTypeAccessor,
            int startFromColumn)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                var value = valueAccessor(property);
                var dataType = dataTypeAccessor(property);

                var cell = GetCellValue(sheetInfo, value, dataType, row.RowIndex!, i + startFromColumn);

                row.AppendChild(cell);
            }
        }

        private static Cell GetCellValue(
            SheetInfo sheetInfo,
            string value,
            CellValues dataType,
            uint rowIndex,
            int columnIndex)
        {
            var cellReference = $"{sheetInfo.ColumnNameProducer.GetColumnName(columnIndex)}{rowIndex}";

            if (dataType == CellValues.SharedString)
            {
                if (!sheetInfo.DocumentInfo.SharedStringIndexCounter.TryGetIndex(value, out var index))
                {
                    _ = sheetInfo
                       .DocumentInfo
                       .SharedStringTable
                       .AppendChild(new SharedStringItem(new Text(value)));

                    index = sheetInfo.DocumentInfo.SharedStringIndexCounter.Next(value);
                }

                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    CellValue = new CellValue(index),
                    StyleIndex = sheetInfo.DocumentInfo.DefaultCellFormatIndex
                };
            }

            if (dataType == CellValues.InlineString)
            {
                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    InlineString = new InlineString(new Text(value)),
                    StyleIndex = sheetInfo.DocumentInfo.DefaultCellFormatIndex
                };
            }

            if (dataType == CellValues.Number)
            {
                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    CellValue = new CellValue(value),
                    StyleIndex = sheetInfo.DocumentInfo.DecimalCellFormatIndex
                };
            }

            if (dataType == CellValues.Date)
            {
                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    CellValue = new CellValue(value),
                    StyleIndex = sheetInfo.DocumentInfo.DateCellFormatIndex
                };
            }

            return new Cell
            {
                CellReference = cellReference,
                DataType = new EnumValue<CellValues>(dataType),
                CellValue = new CellValue(value),
                StyleIndex = sheetInfo.DocumentInfo.DefaultCellFormatIndex
            };
        }

        private static CellValues GetDataType(Type type)
        {
            type = type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

            if (type.IsNumeric())
            {
                return CellValues.Number;
            }

            if (type == typeof(DateTime))
            {
                return CellValues.Date;
            }

            return CellValues.SharedString;
        }

        private static string GetPropertyName(PropertyInfo property)
        {
            return property.Name;
        }

        private static string ConvertValue(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var type = value
               .GetType()
               .ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

            if (type.IsNumeric())
            {
                return Convert
                   .ToDouble(value, CultureInfo.InvariantCulture)
                   .ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(DateTime))
            {
                return ((DateTime)value).ToString(DateTimeFormatString, CultureInfo.InvariantCulture);
            }

            return value.ToString() ?? string.Empty;
        }

        private class DocumentInfo
        {
            public DocumentInfo(
                SharedStringTable sharedStringTable,
                Stylesheet stylesheet,
                uint defaultCellFormatIndex,
                uint decimalCellFormatIndex,
                uint dateCellFormatIndex)
                : this(sharedStringTable,
                    stylesheet,
                    new SharedStringIndexCounter(),
                    defaultCellFormatIndex,
                    decimalCellFormatIndex,
                    dateCellFormatIndex)
            {
            }

            private DocumentInfo(
                SharedStringTable sharedStringTable,
                Stylesheet stylesheet,
                SharedStringIndexCounter sharedStringIndexCounter,
                uint defaultCellFormatIndex,
                uint decimalCellFormatIndex,
                uint dateCellFormatIndex)
            {
                SharedStringTable = sharedStringTable;
                Stylesheet = stylesheet;
                SharedStringIndexCounter = sharedStringIndexCounter;
                DefaultCellFormatIndex = defaultCellFormatIndex;
                DecimalCellFormatIndex = decimalCellFormatIndex;
                DateCellFormatIndex = dateCellFormatIndex;
            }

            public SharedStringTable SharedStringTable { get; }

            public Stylesheet Stylesheet { get; }

            public SharedStringIndexCounter SharedStringIndexCounter { get; }

            public uint DefaultCellFormatIndex { get; }

            public uint DecimalCellFormatIndex { get; }

            public uint DateCellFormatIndex { get; }
        }

        private class SharedStringIndexCounter
        {
            private readonly Dictionary<string, int> _map;

            private int _counter;

            public SharedStringIndexCounter()
            {
                _map = new Dictionary<string, int>(StringComparer.Ordinal);
                _counter = 0;
            }

            public int Next(string stringValue)
            {
                var index = _counter;
                _map.Add(stringValue, index);
                _counter++;
                return index;
            }

            public bool TryGetIndex(string stringValue, out int index)
            {
                return _map.TryGetValue(stringValue, out index);
            }
        }

        private class SheetInfo
        {
            public SheetInfo(DocumentInfo documentInfo)
                : this(documentInfo, new ColumnNameProducer())
            {
            }

            private SheetInfo(
                DocumentInfo documentInfo,
                ColumnNameProducer columnNameProducer)
            {
                DocumentInfo = documentInfo;
                ColumnNameProducer = columnNameProducer;
            }

            public DocumentInfo DocumentInfo { get; }

            public ColumnNameProducer ColumnNameProducer { get; }
        }

        private class ColumnNameProducer
        {
            private readonly IDictionary<int, string> _map;

            public ColumnNameProducer()
            {
                _map = new Dictionary<int, string>();
            }

            public string GetColumnName(int index)
            {
                if (_map.TryGetValue(index, out var value))
                {
                    return value;
                }

                value = index.AlphabetIndex();
                _map[index] = value;

                return value;
            }
        }
    }
}