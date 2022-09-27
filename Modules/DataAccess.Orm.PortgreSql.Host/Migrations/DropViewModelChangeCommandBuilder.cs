namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Host.Model;
    using Sql.Host.Migrations;
    using Sql.Host.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DropViewModelChangeCommandBuilder : IModelChangeCommandBuilder<DropView>,
                                                       IResolvable<IModelChangeCommandBuilder<DropView>>
    {
        private const string CommandFormat = $@"drop materialized view ""{{0}}"".""{{1}}"";

delete from ""{nameof(DataAccess.Orm.Host.Migrations)}"".""{nameof(SqlView)}"" a where a.""{nameof(SqlView.Schema)}"" = '{{0}}' and a.""{nameof(SqlView.View)}"" = '{{1}}'";

        public Task<string> BuildCommand(DropView change, CancellationToken token)
        {
            var commandText = CommandFormat.Format(change.Schema, change.View);

            return Task.FromResult(commandText);
        }
    }
}