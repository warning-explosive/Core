namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class CountLinqExpressionVisitor : ILinqExpressionVisitor,
                                                ICollectionResolvable<ILinqExpressionVisitor>
    {
        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is not System.Linq.Expressions.MethodCallExpression methodCallExpression)
            {
                return false;
            }

            var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.QueryableCount())
            {
                /*
                 * count(*) as "Count"
                 */

                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                visitor.Visit(methodCallExpression.Arguments[0]);
                var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());
                var source = new NamedSourceExpression(itemType, new ParenthesesExpression(context.SqlExpression), parameterExpression);
                var countAllMethodCall = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { new StarExpression() });
                var renameExpression = new RenameExpression(typeof(int), new[] { method }, countAllMethodCall);
                var projectionExpression = new ProjectionExpression(itemType, source, new[] { renameExpression }, null, null);
                context.Remember(projectionExpression);

                return true;
            }

            return false;
        }
    }
}