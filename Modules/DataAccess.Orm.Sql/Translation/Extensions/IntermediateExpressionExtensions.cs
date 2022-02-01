namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Expressions;

    internal static class IntermediateExpressionExtensions
    {
        internal static string Translate(
            this IIntermediateExpression expression,
            IDependencyContainer dependencyContainer,
            int depth)
        {
            return dependencyContainer
                .ResolveGeneric(typeof(IExpressionTranslator<>), expression.GetType())
                .CallMethod(nameof(IExpressionTranslator<IIntermediateExpression>.Translate))
                .WithArguments(expression, depth)
                .Invoke<string>();
        }
    }
}