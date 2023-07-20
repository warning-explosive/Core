namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    internal class DropViewModelChangeCommandBuilder : IModelChangeCommandBuilder<DropView>,
                                                       IResolvable<IModelChangeCommandBuilder<DropView>>,
                                                       ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string DropViewCommandFormat = @"drop materialized view ""{0}"".""{1}""";
        private const string DeleteViewCommandFormat = $@"delete from ""{nameof(Migrations)}"".""{nameof(SqlView)}"" a where a.""{nameof(SqlView.Schema)}"" = @param_0 and a.""{nameof(SqlView.View)}"" = @param_1";

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
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(0.ToString(CultureInfo.InvariantCulture)), change.Schema, typeof(string)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(1.ToString(CultureInfo.InvariantCulture)), change.View, typeof(string))
                });
        }
    }
}