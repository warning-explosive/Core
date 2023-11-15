namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    /// <summary>
    /// AppliedMigration
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Name), Unique = true)]
    public record AppliedMigration : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">PrimaryKey</param>
        /// <param name="dateTime">DateTime</param>
        /// <param name="commandText">CommandText</param>
        /// <param name="name">Name</param>
        public AppliedMigration(
            Guid primaryKey,
            DateTime dateTime,
            string commandText,
            string name)
            : base(primaryKey)
        {
            DateTime = dateTime;
            CommandText = commandText;
            Name = name;
        }

        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// CommandText
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }
}