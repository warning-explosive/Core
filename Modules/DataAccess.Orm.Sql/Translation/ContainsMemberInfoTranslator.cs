namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ContainsMemberInfoTranslator : IMemberInfoTranslator,
                                                  ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (IsEnumerableContains(context)
             || IsICollectionContains(context))
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Contains);
                return true;
            }

            expression = null;
            return false;

            static bool IsEnumerableContains(MemberTranslationContext context)
            {
                return context.Member.DeclaringType == typeof(Enumerable)
                    && context.Member.Name.Equals(nameof(Enumerable.Contains), StringComparison.OrdinalIgnoreCase)
                    && context.Member is MethodInfo method
                    && method.GetParameters().Length == 2;
            }

            static bool IsICollectionContains(MemberTranslationContext context)
            {
                return typeof(ICollection).IsAssignableFrom(context.Member.DeclaringType)
                    && context.Member.Name.Equals(nameof(ICollection<object>.Contains), StringComparison.OrdinalIgnoreCase)
                    && context.Member is MethodInfo method
                    && method.GetParameters().Length == 1;
            }
        }
    }
}