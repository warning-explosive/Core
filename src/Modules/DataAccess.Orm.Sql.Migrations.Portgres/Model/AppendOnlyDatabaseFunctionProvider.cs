namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Model
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class AppendOnlyDatabaseFunctionProvider : IDatabaseFunctionProvider<AppendOnlyAttribute>,
                                                        IResolvable<IDatabaseFunctionProvider<AppendOnlyAttribute>>
    {
        private const string Definition = @"create function ""{0}"".""{1}""() returns trigger
as
$$
begin
    raise exception 'Table is append only that means support only for insert and select commands.';
end;
$$
language plpgsql";

        public string GetDefinition(IReadOnlyDictionary<string, string> context)
        {
            if (context.TryGetValue("schema", out var schema))
            {
                return Definition.Format(schema, nameof(AppendOnlyAttribute));
            }

            throw new InvalidOperationException($"Function {nameof(AppendOnlyAttribute)}() requires schema");
        }
    }
}