namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;
    using Sql.Host.Model;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class DropViewModelChangeCommandBuilder : IModelChangeCommandBuilder<DropView>,
                                                       IResolvable<IModelChangeCommandBuilder<DropView>>,
                                                       ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string DropViewCommandFormat = @"drop materialized view ""{0}"".""{1}""";
        private const string DeleteViewCommandFormat = $@"delete from ""{nameof(Sql.Host.Migrations)}"".""{nameof(SqlView)}"" a where a.""{nameof(SqlView.Schema)}"" = @param_0 and a.""{nameof(SqlView.View)}"" = @param_1";

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is DropView dropView
                ? BuildCommands(dropView)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(DropView change)
        {
            yield return new SqlCommand(
                DropViewCommandFormat.Format(change.Schema, change.View),
                Array.Empty<SqlCommandParameter>());

            yield return new SqlCommand(
                DeleteViewCommandFormat,
                new[]
                {
                    new SqlCommandParameter(TranslationContext.QueryParameterFormat.Format(0.ToString(CultureInfo.InvariantCulture)), change.Schema, typeof(string)),
                    new SqlCommandParameter(TranslationContext.QueryParameterFormat.Format(1.ToString(CultureInfo.InvariantCulture)), change.View, typeof(string))
                });
        }
    }
}