namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : ISqlViewQueryProvider<DatabaseView, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
    gen_random_uuid() as ""{nameof(DatabaseView.PrimaryKey)}"",
    table_name as ""{nameof(DatabaseView.Name)}"",
	view_definition as ""{nameof(DatabaseView.Query)}"",
    table_schema as ""{nameof(DatabaseView.Schema)}""
from information_schema.views";

        public string GetQuery()
        {
            return Query;
        }
    }
}