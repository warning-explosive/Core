namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class SimpleBindingExpressionTranslator : ISqlExpressionTranslator<SimpleBindingExpression>,
                                                       IResolvable<ISqlExpressionTranslator<SimpleBindingExpression>>,
                                                       ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public SimpleBindingExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is SimpleBindingExpression simpleBindingExpression
                ? Translate(simpleBindingExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(SimpleBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var source = expression
               .Flatten()
               .Last()
               .Source;

            sb.Append(_sqlExpressionTranslator.Translate(source, depth));
            sb.Append('.');
            sb.Append('"');
            sb.Append(expression.Name);
            sb.Append('"');

            return sb.ToString();
        }
    }
}