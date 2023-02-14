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
        public bool TryRecognize(
            TranslationContext context,
            MemberInfo member,
            [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (IsEnumerableContains(context, member)
                || IsICollectionContains(context, member))
            {
                expression = new BinaryExpression(typeof(bool), BinaryOperator.Contains);
                return true;
            }

            expression = null;
            return false;

            static bool IsEnumerableContains(TranslationContext context, MemberInfo member)
            {
                return member.DeclaringType == typeof(Enumerable)
                    && member.Name.Equals(nameof(Enumerable.Contains), StringComparison.OrdinalIgnoreCase)
                    && member is MethodInfo method
                    && method.GetParameters().Length == 2;
            }

            static bool IsICollectionContains(TranslationContext context, MemberInfo member)
            {
                return typeof(ICollection).IsAssignableFrom(member.DeclaringType)
                    && member.Name.Equals(nameof(ICollection<object>.Contains), StringComparison.OrdinalIgnoreCase)
                    && member is MethodInfo method
                    && method.GetParameters().Length == 1;
            }
        }
    }
}