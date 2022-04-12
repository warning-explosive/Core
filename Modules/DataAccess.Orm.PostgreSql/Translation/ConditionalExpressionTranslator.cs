namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class ConditionalExpressionTranslator : IExpressionTranslator<ConditionalExpression>,
                                                     IResolvable<IExpressionTranslator<ConditionalExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ConditionalExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(ConditionalExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append("CASE WHEN ");
            sb.Append(expression.When.Translate(_dependencyContainer, depth));
            sb.Append(" THEN ");
            sb.Append(expression.Then.Translate(_dependencyContainer, depth));
            sb.Append(" ELSE ");
            sb.Append(expression.Else.Translate(_dependencyContainer, depth));
            sb.Append(" END");

            return sb.ToString();
        }
    }
}