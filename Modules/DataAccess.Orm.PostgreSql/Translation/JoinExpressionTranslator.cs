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
    internal class JoinExpressionTranslator : IExpressionTranslator<JoinExpression>,
                                              IResolvable<IExpressionTranslator<JoinExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public JoinExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(JoinExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(expression.LeftSource.Translate(_dependencyContainer, depth));
            sb.AppendLine("JOIN");
            sb.Append(new string('\t', depth));
            sb.AppendLine(expression.RightSource.Translate(_dependencyContainer, depth));
            sb.AppendLine("ON");
            sb.Append(new string('\t', depth));
            sb.Append(expression.On.Translate(_dependencyContainer, depth));

            return sb.ToString();
        }
    }
}