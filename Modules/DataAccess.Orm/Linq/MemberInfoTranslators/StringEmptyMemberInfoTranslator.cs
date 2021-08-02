namespace SpaceEngineers.Core.DataAccess.Orm.Linq.MemberInfoTranslators
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
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
                expression = new QueryParameterExpression(typeof(string), context.NextQueryParameterName(), "''");
                return true;
            }

            expression = null;
            return false;
        }
    }
}