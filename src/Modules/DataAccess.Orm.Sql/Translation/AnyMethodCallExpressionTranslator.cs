namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class SelectAnyLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.QueryableAny())
            {
                /*
                 * count(*) > 0 as "Any"
                 */

                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                visitor.Visit(methodCallExpression.Arguments[0]);
                var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());
                ISqlExpression source = new NamedSourceExpression(itemType, new ParenthesesExpression(context.SqlExpression), parameterExpression);
                var left = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { new StarExpression() });
                var name = context.NextCommandParameterName();
                var extractor = new Func<CommandParameterExtractionContext, ConstantExpression>(static _ => Expression.Constant(0, typeof(int)));
                context.CaptureCommandParameterExtractor(name, extractor);
                var right = new QueryParameterExpression(typeof(int), name);
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.GreaterThan, left, right);
                var parenthesesExpression = new ParenthesesExpression(binaryExpression);
                var renameExpression = new RenameExpression(typeof(bool), method.Name, parenthesesExpression);
                var projectionExpression = new ProjectionExpression(itemType, source, new[] { renameExpression }, null, null);
                context.Remember(projectionExpression);

                return true;
            }

            return false;
        }
    }
}