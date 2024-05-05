namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class StringLengthLinqExpressionVisitor : ILinqExpressionVisitor,
                                                       ICollectionResolvable<ILinqExpressionVisitor>
    {
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is MemberExpression memberExpression
                && memberExpression.Member == LinqMethods.StringLength())
            {
                visitor.Visit(memberExpression.Expression);
                var argument = context.SqlExpression;
                var methodCallExpression = new Expressions.MethodCallExpression(typeof(int), nameof(string.Length).ToLowerInvariant(), null, new[] { argument });
                context.Remember(methodCallExpression);

                return true;
            }

            return false;
        }
    }
}