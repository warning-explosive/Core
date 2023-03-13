namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Host.Model;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseFunctionViewQueryProvider : ISqlViewQueryProvider<DatabaseFunction, Guid>,
                                                       IResolvable<ISqlViewQueryProvider<DatabaseFunction, Guid>>,
                                                       ICollectionResolvable<ISqlViewQueryProvider>
    {
        private const string Query = $@"select
gen_random_uuid() as ""{nameof(DatabaseFunction.PrimaryKey)}"",
ns.nspname as ""{nameof(DatabaseFunction.Schema)}"",
pr.proname as ""{nameof(DatabaseFunction.Function)}"",
functionView.""{nameof(FunctionView.Definition)}"" as ""{nameof(DatabaseFunction.Definition)}""
from pg_proc pr
join pg_type tp on tp.oid = pr.prorettype
join pg_namespace ns on ns.oid = pr.pronamespace
join ""{nameof(Sql.Host.Migrations)}"".""{nameof(FunctionView)}"" functionView
on ns.nspname = functionView.""{nameof(FunctionView.Schema)}"" and pr.proname = functionView.""{nameof(FunctionView.Function)}""
where ns.nspname not in ('information_schema', 'public') and ns.nspname not like 'pg_%'";

        public string GetQuery()
        {
            return Query;
        }
    }
}