namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class StringLengthSqlExpressionProvider : ISqlExpressionProvider
    {
        public bool TryRecognize(MemberInfo member, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            if (member.DeclaringType == typeof(string)
                && member.Name.Equals(nameof(string.Length), StringComparison.OrdinalIgnoreCase))
            {
                expression = new MethodCallExpression(typeof(int), nameof(string.Length).ToLowerInvariant(), Enumerable.Empty<IIntermediateExpression>());
                return true;
            }

            expression = null;
            return false;
        }
    }
}