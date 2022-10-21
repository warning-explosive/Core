namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>,
                                                     IResolvable<IExpressionTranslator<QuerySourceExpression>>
    {
        private readonly IModelProvider _modelProvider;

        public QuerySourceExpressionTranslator(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append('"');
            sb.Append(_modelProvider.SchemaName(expression.Type));
            sb.Append('"');
            sb.Append('.');
            sb.Append('"');
            sb.Append(_modelProvider.TableName(expression.Type));
            sb.Append('"');

            return sb.ToString();
        }
    }
}