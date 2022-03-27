namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    /// <summary>
    /// EnInsertBehavior
    /// </summary>
    public enum EnInsertBehavior
    {
        /// <summary>
        /// Default insert behavior
        /// </summary>
        Default = 0,

        /// <summary>
        /// On conflict do nothing
        /// </summary>
        DoNothing = 1,

        /// <summary>
        /// On conflict do update (INSERT || UPDATE = UPSERT)
        /// </summary>
        DoUpdate = 2
    }
}