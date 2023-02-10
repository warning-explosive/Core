namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System;
    using System.Data;

    /// <summary>
    /// IDatabaseConnection
    /// </summary>
    public interface IDatabaseConnection : IDisposable
    {
        /// <summary>
        /// Gets underlying db connection
        /// </summary>
        IDbConnection DbConnection { get; }
    }
}