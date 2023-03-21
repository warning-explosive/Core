namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class StringEmptyUnknownExpressionTranslator : IUnknownExpressionTranslator,
                                                            ICollectionResolvable<IUnknownExpressionTranslator>
    {
        public bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor)
        {
            if (expression is MemberExpression memberExpression
                && memberExpression.Member == LinqMethods.StringEmpty())
            {
                context.WithinScope(
                    new QueryParameterExpression(context, typeof(string), static (_, _) => Expression.Constant(string.Empty, typeof(string))),
                    () => visitor.Visit(memberExpression.Expression));

                return true;
            }

            return false;
        }
    }
}