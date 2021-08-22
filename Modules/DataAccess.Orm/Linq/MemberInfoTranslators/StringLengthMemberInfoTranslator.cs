namespace SpaceEngineers.Core.DataAccess.Orm.Linq.MemberInfoTranslators
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class StringLengthMemberInfoTranslator : IMemberInfoTranslator,
                                                      ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            if (context.Member.DeclaringType == typeof(string)
                && context.Member.Name.Equals(nameof(string.Length), StringComparison.OrdinalIgnoreCase))
            {
                expression = new MethodCallExpression(typeof(int), nameof(string.Length).ToLowerInvariant(), null, Enumerable.Empty<IIntermediateExpression>());
                return true;
            }

            expression = null;
            return false;
        }
    }
}