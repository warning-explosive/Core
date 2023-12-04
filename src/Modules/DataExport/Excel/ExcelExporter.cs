namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.IO;
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
        private static readonly MethodInfo FillFlatTableGenericMethod = new MethodFinder(
            typeof(ExcelExporter),
            nameof(FillFlatTableGeneric),
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod)
        {
            TypeArguments = new[] { typeof(ISheetInfo) },
            ArgumentTypes = new[] { typeof(FlatTableSheetInfo<ISheetInfo>), typeof(SheetMetadata), typeof(SheetData) }
        }.FindMethod() ?? throw new InvalidOperationException($"Could not find {nameof(ExcelExporter)}.{nameof(FillFlatTableGeneric)}() method");

        private static readonly MethodInfo FillPivotTableGenericMethod = new MethodFinder(
            typeof(ExcelExporter),
            nameof(FillPivotTableGeneric),
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod)
        {
            TypeArguments = new[] { typeof(ISheetInfo) },
            ArgumentTypes = new[] { typeof(PivotTableSheetInfo<ISheetInfo>), typeof(SheetMetadata), typeof(SheetData) }
        }.FindMethod() ?? throw new InvalidOperationException($"Could not find {nameof(ExcelExporter)}.{nameof(FillPivotTableGeneric)}() method");

        public Stream ExportXlsx(ISheetInfo[] sheets)
        {
            var stream = new MemoryStream();

            try
            {
                FillStream(stream, sheets);

                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
        }

        private static void FillStream(MemoryStream stream, ISheetInfo[] sheets)
        {
            using var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var workbookSheets = workbookPart.Workbook.AppendChild(new Sheets());

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

            for (uint i = 0; i < sheets.Length; i++)
            {
                var sheetInfo = sheets[i];
                var sheetMetadata = new SheetMetadata(documentInfo);

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet();

                var workbookSheet = new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = i + 1,
                    Name = sheetInfo.SheetName
                };

                workbookSheets.AppendChild(workbookSheet);

                var type = sheetInfo.GetType();

                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(FlatTableSheetInfo<>))
                {
                    FillFlatTable(sheetInfo, sheetMetadata, sheetData);
                }
                else if (type.IsGenericType
                         && type.GetGenericTypeDefinition() == typeof(PivotTableSheetInfo<>))
                {
                    FillPivotTable(sheetInfo, sheetMetadata, sheetData);
                }
                else
                {
                    throw new NotSupportedException(type.FullName);
                }

                worksheetPart.Worksheet.Append(sheetData);
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
            ISheetInfo sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData)
        {
            _ = FillFlatTableGenericMethod
                .MakeGenericMethod(sheetInfo.GetType().GetGenericArguments())
                .Invoke(null, new object?[] { sheetInfo, sheetMetadata, sheetData });
        }

        private static void FillFlatTableGeneric<TRow>(
            FlatTableSheetInfo<TRow> sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData)
        {
            new FlatTableSheetExporter<TRow>().Fill(sheetInfo, sheetMetadata, sheetData);
        }

        private static void FillPivotTable(
            ISheetInfo sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData)
        {
            _ = FillPivotTableGenericMethod
                .MakeGenericMethod(sheetInfo.GetType().GetGenericArguments())
                .Invoke(null, new object?[] { sheetInfo, sheetMetadata, sheetData });
        }

        private static void FillPivotTableGeneric<TRow>(
            PivotTableSheetInfo<TRow> sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData)
        {
            new PivotTableSheetExporter<TRow>().Fill(sheetInfo, sheetMetadata, sheetData);
        }
    }
}