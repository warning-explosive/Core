namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    /// <summary>
    /// ITranslatedQuery
    /// </summary>
    public interface ITranslatedQuery
    {
        /// <summary>
        /// Query
        /// </summary>
        string Query { get; }

        /// <summary>
        /// Query parameters object
        /// </summary>
        object? Parameters { get; }
    }
}