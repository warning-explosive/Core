namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ColumnsChainExpressionTranslator : ISqlExpressionTranslator<ColumnsChainExpression>,
                                                      IResolvable<ISqlExpressionTranslator<ColumnsChainExpression>>,
                                                      ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ColumnsChainExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ColumnsChainExpression columnsChainExpression
                ? Translate(columnsChainExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ColumnsChainExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (expression.Source is not ParameterExpression { SkipInSql: true })
            {
                sb.Append(_translator.Translate(expression.Source, depth));
                sb.Append('.');
            }

            sb.Append('"');
            sb.Append(string.Join("_", expression.Members.Select(member => member.Name)));
            sb.Append('"');

            return sb.ToString();
        }
    }
}