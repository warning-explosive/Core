namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectEqualsMemberInfoTranslator : IMemberInfoTranslator,
                                                      ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(
            TranslationContext context,
            MemberInfo member,
            [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (member.DeclaringType == typeof(object)
                && member.Name.Equals(nameof(object.Equals), StringComparison.OrdinalIgnoreCase))
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Equal);
                return true;
            }

            expression = null;
            return false;
        }
    }
}