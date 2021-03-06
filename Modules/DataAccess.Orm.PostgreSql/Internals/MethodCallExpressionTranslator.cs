namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using System.Linq;
    using System.Text;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class MethodCallExpressionTranslator : IExpressionTranslator<MethodCallExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public MethodCallExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
        public string Translate(MethodCallExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(expression.Name);
            sb.Append('(');
            sb.Append(expression
                .Arguments
                .Select(argument => argument.Translate(_dependencyContainer, depth))
                .ToString(", "));
            sb.Append(')');

            return sb.ToString();
        }
    }
}