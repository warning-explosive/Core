namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Reading;

    [Component(EnLifestyle.Singleton)]
    internal class LikeMemberInfoTranslator : IMemberInfoTranslator,
                                              ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            if (context.Member.DeclaringType == typeof(SqlExpressionsExtensions)
                && context.Member.Name.Equals(nameof(SqlExpressionsExtensions.Like), StringComparison.OrdinalIgnoreCase))
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Like);

                return true;
            }

            expression = null;
            return false;
        }
    }
}