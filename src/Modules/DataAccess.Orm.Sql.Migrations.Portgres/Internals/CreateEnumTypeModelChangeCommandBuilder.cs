namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Postgres.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateEnumTypeModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateEnumType>,
                                                             IResolvable<CreateColumnModelChangeCommandBuilder>,
                                                             IResolvable<IModelChangeCommandBuilder<CreateEnumType>>,
                                                             ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create type ""{0}"".""{1}"" as enum ({2})";
        private const string ValueFormat = @"'{0}'";

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateEnumType createEnumType
                ? BuildCommands(createEnumType)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        [SuppressMessage("Analysis", "CA1308", Justification = "Custom enum types in postgres are case sensitive. Npgssql translates custom enum value to lower case.")]
        public IEnumerable<ICommand> BuildCommands(CreateEnumType change)
        {
            var values = change
                .Values
                .Select(value => ValueFormat.Format(value.ToLowerInvariant()))
                .ToString(", ");

            yield return new SqlCommand(
                CommandFormat.Format(change.Schema, change.Type, values),
                Array.Empty<SqlCommandParameter>());
        }
    }
}