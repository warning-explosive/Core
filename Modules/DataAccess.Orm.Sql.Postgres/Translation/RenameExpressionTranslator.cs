namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class RenameExpressionTranslator : ISqlExpressionTranslator<RenameExpression>,
                                                IResolvable<ISqlExpressionTranslator<RenameExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public RenameExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is RenameExpression renameExpression
                ? Translate(renameExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(RenameExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var parentheses = expression.Source is not ColumnExpression;

            if (parentheses)
            {
                sb.Append('(');
            }

            sb.Append(_translator.Translate(expression.Source, depth));

            if (parentheses)
            {
                sb.Append(')');
            }

            sb.Append(" AS ");
            sb.Append('"');
            sb.Append(expression.Name);
            sb.Append('"');

            return sb.ToString();
        }
    }
}