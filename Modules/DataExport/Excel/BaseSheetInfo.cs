namespace SpaceEngineers.Core.DataExport.Excel
{
    /// <summary>
    /// BaseSheetInfo
    /// </summary>
    public abstract class BaseSheetInfo : ISheetInfo
    {
        /// <summary> .cctor </summary>
        protected BaseSheetInfo()
        {
            SheetName = "Sheet1";
        }

        /// <inheritdoc />
        public string SheetName { get; set; }
    }
}