namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq;
    using MethodCallExpression = Expressions.MethodCallExpression;

    [Component(EnLifestyle.Singleton)]
    internal class StringLengthUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                             ICollectionResolvable<IUnknownExpressionTranslator>
    {
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MemberExpression memberExpression
                && memberExpression.Member == LinqMethods.StringLength())
            {
                visitor.Visit(memberExpression.Expression);
                var argument = context.SqlExpression;
                var methodCallExpression = new MethodCallExpression(typeof(int), nameof(string.Length).ToLowerInvariant(), null, new[] { argument });
                context.Remember(methodCallExpression);

                return true;
            }

            return false;
        }
    }
}