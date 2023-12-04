namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Deduplication
{
    /// <summary>
    /// EnInboxMessageState
    /// </summary>
    public enum EnInboxMessageState
    {
        /// <summary>
        /// In active processing phase
        /// </summary>
        Processing = 0,

        /// <summary>
        /// Handled successfully
        /// </summary>
        Handled = 1,

        /// <summary>
        /// Handled with error (after applied retry policy)
        /// </summary>
        Failed = 2
    }
}