namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateFunctionModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateFunction>,
                                                             IResolvable<IModelChangeCommandBuilder<CreateFunction>>,
                                                             ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string InsertFunctionCommandFormat = $@"insert into ""{nameof(Sql.Host.Migrations)}"".""{nameof(FunctionView)}""(""{nameof(FunctionView.PrimaryKey)}"", ""{nameof(FunctionView.Version)}"", ""{nameof(FunctionView.Schema)}"", ""{nameof(FunctionView.Function)}"", ""{nameof(FunctionView.Definition)}"") values (@param_0, @param_1, @param_2, @param_3, @param_4)";

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateFunction createFunction
                ? BuildCommands(createFunction)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateFunction change)
        {
            yield return new SqlCommand(
                change.CommandText,
                Array.Empty<SqlCommandParameter>());

            var functionView = new FunctionView(Guid.NewGuid(), change.Schema, change.Function, change.CommandText);

            yield return new SqlCommand(
                InsertFunctionCommandFormat,
                new[]
                {
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(0.ToString(CultureInfo.InvariantCulture)), functionView.PrimaryKey, typeof(Guid)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(1.ToString(CultureInfo.InvariantCulture)), functionView.Version, typeof(long)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(2.ToString(CultureInfo.InvariantCulture)), functionView.Schema, typeof(string)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(3.ToString(CultureInfo.InvariantCulture)), functionView.Function, typeof(string)),
                    new SqlCommandParameter(TranslationContext.CommandParameterFormat.Format(4.ToString(CultureInfo.InvariantCulture)), functionView.Definition, typeof(string))
                });
        }
    }
}