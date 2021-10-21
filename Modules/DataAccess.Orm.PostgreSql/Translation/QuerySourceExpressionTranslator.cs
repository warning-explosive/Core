namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public QuerySourceExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (expression.Type.IsSqlView())
            {
                // TODO: #110 - inline only on database model initialisation, after use created view object as normal table
                sb.Append("(");
                sb.Append(expression.Type.SqlViewQuery(_dependencyContainer));
                sb.Append(")");
            }
            else
            {
                sb.Append('\"');
                sb.Append(expression.Type.SchemaName());
                sb.Append('\"');
                sb.Append('.');
                sb.Append('\"');
                sb.Append(expression.Type.Name);
                sb.Append('\"');
            }

            return sb.ToString();
        }
    }
}