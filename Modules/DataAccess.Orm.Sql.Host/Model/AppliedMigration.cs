namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using Api.Model;

    [Schema(nameof(DataAccess.Orm.Host.Migrations))]
    [Index(nameof(Name), Unique = true)]
    internal record AppliedMigration : BaseDatabaseEntity<Guid>
    {
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

        public DateTime DateTime { get; set; }
        public string CommandText { get; set; }
        public string Name { get; set; }
    }
}