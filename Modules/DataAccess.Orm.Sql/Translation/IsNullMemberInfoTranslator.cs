namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class IsNullMemberInfoTranslator : IMemberInfoTranslator,
                                                ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(
            TranslationContext context,
            MemberInfo member,
            [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (member == SqlLinqMethods.IsNull())
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Is, default!, new QueryParameterExpression(context, typeof(object), static _ => null));
                return true;
            }

            expression = null;
            return false;
        }
    }
}