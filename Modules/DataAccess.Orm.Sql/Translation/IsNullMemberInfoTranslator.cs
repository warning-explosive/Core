namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Linq;

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
                var type = ((System.Linq.Expressions.IArgumentProvider)context.Node!).GetArgument(0).Type;

                expression = new BinaryExpression(
                    typeof(bool),
                    BinaryOperator.Is,
                    default!,
                    new SpecialExpression("NULL"));

                return true;
            }

            expression = null;
            return false;
        }
    }
}