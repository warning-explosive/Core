namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
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
                var name = context.NextCommandParameterName();
                var extractor = new Func<CommandParameterExtractionContext, ConstantExpression>(static _ => Expression.Constant(string.Empty, typeof(string)));
                context.CaptureCommandParameterExtractor(name, extractor);
                var queryParameterExpression = new QueryParameterExpression(typeof(string), name);
                context.Remember(queryParameterExpression);

                return true;
            }

            return false;
        }
    }
}