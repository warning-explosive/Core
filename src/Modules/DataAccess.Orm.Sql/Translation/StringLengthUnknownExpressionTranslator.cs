namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
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
                context.WithinScope(
                    new MethodCallExpression(typeof(int), nameof(string.Length).ToLowerInvariant(), null, Enumerable.Empty<ISqlExpression>()),
                    () => visitor.Visit(memberExpression.Expression));

                return true;
            }

            return false;
        }
    }
}