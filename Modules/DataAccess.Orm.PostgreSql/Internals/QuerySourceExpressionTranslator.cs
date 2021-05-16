namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using System.Text;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using ValueObjects;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>
    {
        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append("todo_database.todo_schema.");
            sb.Append(expression.ItemType.Name);

            return sb.ToString();
        }
    }
}