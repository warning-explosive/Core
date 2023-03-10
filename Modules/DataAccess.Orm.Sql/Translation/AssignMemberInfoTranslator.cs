namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class AssignMemberInfoTranslator : IMemberInfoTranslator,
                                                ICollectionResolvable<IMemberInfoTranslator>
    {
        public bool TryRecognize(
            TranslationContext context,
            MemberInfo member,
            [NotNullWhen(true)] out ISqlExpression? expression)
        {
            if (member is MethodInfo methodInfo
                && methodInfo.GenericMethodDefinitionOrSelf() == LinqMethods.Assign())
            {
                expression = new BinaryExpression(
                    typeof(void),
                    BinaryOperator.Assign,
                    default!,
                    default!);

                return true;
            }

            expression = null;
            return false;
        }
    }
}