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
    internal class AllLinqExpressionVisitor : ILinqExpressionVisitor,
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

            if (method == LinqMethods.QueryableAll())
            {
                /*
                 * (count(case when <condition> then 1 else null end) = count(*)) as "All"
                 */

                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                visitor.Visit(methodCallExpression.Arguments[0]);
                var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());
                var source = new NamedSourceExpression(itemType, context.SqlExpression, parameterExpression);

                using (context.OpenParametersScope(parameterExpression))
                {
                    visitor.Visit(methodCallExpression.Arguments[1]);
                }

                var when = context.SqlExpression;
                var name = context.NextCommandParameterName();
                var extractor = new Func<CommandParameterExtractionContext, ConstantExpression>(static _ => Expression.Constant(1, typeof(int)));
                context.CaptureCommandParameterExtractor(name, extractor);
                var then = new QueryParameterExpression(typeof(int), name);
                var @else = new NullExpression();
                var conditionalExpression = new Expressions.ConditionalExpression(typeof(int), when, then, @else);
                var left = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { conditionalExpression });
                var right = new Expressions.MethodCallExpression(typeof(int), nameof(Queryable.Count), null, new[] { new StarExpression() });
                var binaryExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, left, right);
                var parenthesesExpression = new ParenthesesExpression(binaryExpression);
                var renameExpression = new RenameExpression(typeof(bool), new[] { method }, parenthesesExpression);
                var projectionExpression = new ProjectionExpression(itemType, source, new[] { renameExpression }, null, null);
                context.Remember(projectionExpression);

                return true;
            }

            return false;
        }
    }
}