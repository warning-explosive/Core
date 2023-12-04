namespace SpaceEngineers.Core.DataExport.Excel
{
    using DocumentFormat.OpenXml.Spreadsheet;

    internal class DocumentInfo
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
}