namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class GroupByExpressionTranslator : IExpressionTranslator<GroupByExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GroupByExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(GroupByExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(new string('\t', depth - 1));
            sb.AppendLine(expression.Source.Translate(_dependencyContainer, depth));

            sb.Append(new string('\t', depth - 1));
            sb.AppendLine("GROUP BY");

            sb.Append(expression
                .Key
                .Bindings
                .Select(binding =>
                {
                    var keyBuilder = new StringBuilder();

                    keyBuilder.Append(new string('\t', depth));
                    keyBuilder.Append(binding.Translate(_dependencyContainer, depth));

                    return keyBuilder.ToString();
                })
                .ToString($",{Environment.NewLine}"));

            return sb.ToString();
        }
    }
}