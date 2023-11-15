namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;

    /// <summary>
    /// EnTriggerEvent
    /// </summary>
    [Flags]
    public enum EnTriggerEvent
    {
        /// <summary>
        /// Insert
        /// </summary>
        Insert = 1 << 0,

        /// <summary>
        /// Update
        /// </summary>
        Update = 1 << 1,

        /// <summary>
        /// Delete
        /// </summary>
        Delete = 1 << 2
    }
}