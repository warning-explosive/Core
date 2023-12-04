namespace SpaceEngineers.Core.DataExport.Excel
{
    using DocumentFormat.OpenXml.Spreadsheet;

    internal interface IExcelSheetExporter<TSheetInfo>
        where TSheetInfo : ISheetInfo
    {
        void Fill(
            TSheetInfo sheetInfo,
            SheetMetadata sheetMetadata,
            SheetData sheetData);
    }
}