namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Model;

    internal record AppliedMigration : BaseDatabaseEntity<Guid>
    {
        public AppliedMigration(
            Guid primaryKey,
            DateTime dateTime,
            string commandText)
            : base(primaryKey)
        {
            DateTime = dateTime;
            CommandText = commandText;
        }

        public DateTime DateTime { get; private init; }
        public string CommandText { get; private init; }
    }
}