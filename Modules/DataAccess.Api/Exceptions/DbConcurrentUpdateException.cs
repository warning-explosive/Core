namespace SpaceEngineers.Core.DataAccess.Api.Exceptions
{
    using System;

    /// <summary>
    /// DbConcurrentUpdateException
    /// </summary>
    public sealed class DbConcurrentUpdateException : Exception
    {
        /// <summary> .cctor </summary>
        /// <param name="entity">Entity type</param>
        public DbConcurrentUpdateException(Type entity)
            : base($"Entity {entity} have been tried to update concurrently")
        {
        }
    }
}