namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DropViewModelChangeCommandBuilder : IModelChangeCommandBuilder<DropView>,
                                                       IResolvable<IModelChangeCommandBuilder<DropView>>,
                                                       ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = $@"drop materialized view ""{{0}}"".""{{1}}"";

delete from ""{nameof(Sql.Host.Migrations)}"".""{nameof(SqlView)}"" a where a.""{nameof(SqlView.Schema)}"" = '{{0}}' and a.""{nameof(SqlView.View)}"" = '{{1}}'";

        public Task<string> BuildCommand(IModelChange change, CancellationToken token)
        {
            return change is DropView dropView
                ? BuildCommand(dropView, token)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public Task<string> BuildCommand(DropView change, CancellationToken token)
        {
            var commandText = CommandFormat.Format(change.Schema, change.View);

            return Task.FromResult(commandText);
        }
    }
}