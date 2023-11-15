namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class InsertExpressionTranslator : ISqlExpressionTranslator<InsertExpression>,
                                                IResolvable<ISqlExpressionTranslator<InsertExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private const string ColumnFormat = @"""{0}""";
        private const string AssignExpressionFormat = @"{0} = {1}";
        private const string ExcludedPseudoColumnFormat = @"excluded.{0}";
        private const string OnConflictDoNothing = @" on conflict do nothing";
        private const string OnConflictDoUpdate = @" on conflict ({0}) do update set {1}";

        private readonly IModelProvider _modelProvider;
        private readonly ISqlExpressionTranslatorComposite _translator;

        public InsertExpressionTranslator(
            IModelProvider modelProvider,
            ISqlExpressionTranslatorComposite translator)
        {
            _modelProvider = modelProvider;
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is InsertExpression insertExpression
                ? Translate(insertExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(InsertExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var table = _modelProvider.Tables[expression.Type];

            sb.Append(CultureInfo.InvariantCulture, $@"INSERT INTO ""{table.Schema}"".""{table.Name}"" ");

            sb.Append('(');

            sb.Append(table
                .Columns
                .Values
                .Where(column => !column.IsMultipleRelation)
                .Select(column => ColumnFormat.Format(column.Name))
                .ToString(", "));

            sb.AppendLine(")");

            sb.AppendLine("VALUES");

            sb.Append(expression
                .Values
                .Select(values => _translator.Translate(values, depth))
                .ToString("," + Environment.NewLine + new string('\t', depth)));

            sb.Append(ApplyInsertBehavior(table, expression.InsertBehavior));

            return sb.ToString();
        }

        private static string ApplyInsertBehavior(
            ITableInfo table,
            EnInsertBehavior insertBehavior)
        {
            return insertBehavior switch
            {
                EnInsertBehavior.Default => string.Empty,
                EnInsertBehavior.DoNothing => OnConflictDoNothing,
                EnInsertBehavior.DoUpdate => ApplyUpdateInsertBehavior(table),
                _ => throw new NotSupportedException(insertBehavior.ToString())
            };

            static string ApplyUpdateInsertBehavior(ITableInfo table)
            {
                if (table.IsMtmTable)
                {
                    return OnConflictDoNothing;
                }

                var hasOnlyPrimaryKey = table
                    .Columns
                    .Values
                    .Where(column => !column.IsMultipleRelation)
                    .All(column => column.Name.Equals(nameof(IUniqueIdentified.PrimaryKey), StringComparison.OrdinalIgnoreCase));

                if (hasOnlyPrimaryKey)
                {
                    return OnConflictDoNothing;
                }

                return OnConflictDoUpdate.Format(KeyColumns(table), Update(table));

                static string KeyColumns(ITableInfo table)
                {
                    var uniqueIndexColumns = table
                        .Indexes
                        .Values
                        .SingleOrDefault(index => index.Unique)
                       ?.Columns
                        .Select(column => column.Name) ?? new[] { nameof(IDatabaseEntity.PrimaryKey) };

                    return uniqueIndexColumns.Select(column => ColumnFormat.Format(column)).ToString(", ");
                }

                static string Update(ITableInfo table)
                {
                    return table
                        .Columns
                        .Values
                        .Where(column => !column.IsMultipleRelation)
                        .Select(column => AssignExpressionFormat.Format(ColumnFormat.Format(column.Name), ExcludedPseudoColumnFormat.Format(ColumnFormat.Format(column.Name))))
                        .ToString("," + Environment.NewLine);
                }
            }
        }
    }
}