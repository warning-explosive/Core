namespace SpaceEngineers.Core.DataAccess.Api.Transaction
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
        IDbConnection UnderlyingDbConnection { get; }
    }
}