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
    internal class StringEmptyMemberInfoTranslator : IMemberInfoTranslator,
                                                     ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(
            TranslationContext context,
            MemberInfo member,
            [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (member.DeclaringType == typeof(string)
                && member.Name.Equals(nameof(string.Empty), StringComparison.OrdinalIgnoreCase))
            {
                expression = new QueryParameterExpression(
                    context,
                    typeof(string),
                    static _ => System.Linq.Expressions.Expression.Constant(string.Empty, typeof(string)));
                return true;
            }

            expression = null;
            return false;
        }
    }
}