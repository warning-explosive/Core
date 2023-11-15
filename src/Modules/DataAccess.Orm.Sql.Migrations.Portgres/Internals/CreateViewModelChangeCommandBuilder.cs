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
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateViewModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateView>,
                                                         IResolvable<IModelChangeCommandBuilder<CreateView>>,
                                                         ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CreateViewCommandFormat = @"create materialized view ""{0}"".""{1}"" as {2}";
        private const string InsertViewCommandFormat = $@"insert into ""{nameof(Migrations)}"".""{nameof(SqlView)}""(""{nameof(SqlView.PrimaryKey)}"", ""{nameof(SqlView.Version)}"", ""{nameof(SqlView.Schema)}"", ""{nameof(SqlView.View)}"", ""{nameof(SqlView.Query)}"") values (@param_0, @param_1, @param_2, @param_3, @param_4)";

        private readonly IModelProvider _modelProvider;

        public CreateViewModelChangeCommandBuilder(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateView createView
                ? BuildCommands(createView)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateView change)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.View, out var info)
                || info is not ViewInfo view)
            {
                throw new InvalidOperationException($"{change.Schema}.{change.View} isn't presented in the model");
            }

            yield return new SqlCommand(
                CreateViewCommandFormat.Format(change.Schema, change.View, view.Query),
                Array.Empty<SqlCommandParameter>());

            var sqlView = new SqlView(Guid.NewGuid(), change.Schema, change.View, view.Query);

            yield return new SqlCommand(
                InsertViewCommandFormat,
                new[]
                {
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(0.ToString(CultureInfo.InvariantCulture)), sqlView.PrimaryKey, typeof(Guid)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(1.ToString(CultureInfo.InvariantCulture)), sqlView.Version, typeof(long)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(2.ToString(CultureInfo.InvariantCulture)), sqlView.Schema, typeof(string)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(3.ToString(CultureInfo.InvariantCulture)), sqlView.View, typeof(string)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(4.ToString(CultureInfo.InvariantCulture)), sqlView.Query, typeof(string))
                });
        }
    }
}