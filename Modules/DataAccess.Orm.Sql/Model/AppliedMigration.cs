namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;

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

        public DateTime DateTime { get; private init; }
        public string CommandText { get; private init; }
        public string Name { get; private init; }
    }
}