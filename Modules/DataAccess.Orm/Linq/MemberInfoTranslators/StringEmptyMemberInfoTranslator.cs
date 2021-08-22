namespace SpaceEngineers.Core.DataAccess.Orm.Linq.MemberInfoTranslators
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class StringEmptyMemberInfoTranslator : IMemberInfoTranslator,
                                                     ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            if (context.Member.DeclaringType == typeof(string)
                && context.Member.Name.Equals(nameof(string.Empty), StringComparison.OrdinalIgnoreCase))
            {
                expression = QueryParameterExpression.Create(context, typeof(string), string.Empty);
                return true;
            }

            expression = null;
            return false;
        }
    }
}