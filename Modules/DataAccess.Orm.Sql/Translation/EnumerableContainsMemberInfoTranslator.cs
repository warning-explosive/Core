namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class EnumerableContainsMemberInfoTranslator : IMemberInfoTranslator,
                                                            ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out IIntermediateExpression? expression)
        {
            if (context.Member.DeclaringType == typeof(Enumerable)
                && context.Member.Name.Equals(nameof(Enumerable.Contains), StringComparison.OrdinalIgnoreCase)
                && context.Member is MethodInfo method
                && method.GetParameters().Length == 2)
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Contains);

                return true;
            }

            expression = null;
            return false;
        }
    }
}