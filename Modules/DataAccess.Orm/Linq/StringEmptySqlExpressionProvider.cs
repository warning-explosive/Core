namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class StringEmptySqlExpressionProvider : ISqlExpressionProvider,
                                                      ICollectionResolvable<ISqlExpressionProvider>
    {
        public bool TryRecognize(MemberInfo member, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            if (member.DeclaringType == typeof(string)
                && member.Name.Equals(nameof(string.Empty), StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConstantExpression(typeof(string), "''");
                return true;
            }

            expression = null;
            return false;
        }
    }
}