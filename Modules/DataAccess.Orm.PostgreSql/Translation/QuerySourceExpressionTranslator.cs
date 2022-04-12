namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>,
                                                     IResolvable<IExpressionTranslator<QuerySourceExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IModelProvider _modelProvider;

        public QuerySourceExpressionTranslator(
            IDependencyContainer dependencyContainer,
            IModelProvider modelProvider)
        {
            _dependencyContainer = dependencyContainer;
            _modelProvider = modelProvider;
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (expression.Type.IsSqlView())
            {
                // TODO: #110 - inline only on database model initialisation, after use created view object as normal table
                sb.Append('(');
                sb.Append(expression.Type.SqlViewQuery(_dependencyContainer));
                sb.Append(')');
            }
            else
            {
                sb.Append('"');
                sb.Append(_modelProvider.SchemaName(expression.Type));
                sb.Append('"');
                sb.Append('.');
                sb.Append('"');
                sb.Append(_modelProvider.TableName(expression.Type));
                sb.Append('"');
            }

            return sb.ToString();
        }
    }
}