namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectEqualsMemberInfoTranslator : IMemberInfoTranslator,
                                                      ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (context.Member.DeclaringType == typeof(object)
                && context.Member.Name.Equals(nameof(object.Equals), StringComparison.OrdinalIgnoreCase))
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Equal);
                return true;
            }

            expression = null;
            return false;
        }
    }
}