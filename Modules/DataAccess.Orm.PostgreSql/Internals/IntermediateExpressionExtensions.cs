namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Basics;

    internal static class IntermediateExpressionExtensions
    {
        internal static string Translate(
            this IIntermediateExpression expression,
            IDependencyContainer dependencyContainer,
            int depth)
        {
            var service = typeof(IExpressionTranslator<>).MakeGenericType(expression.GetType());

            return dependencyContainer
                .Resolve(service)
                .CallMethod(nameof(IExpressionTranslator<IIntermediateExpression>.Translate))
                .WithArguments(expression, depth)
                .Invoke<string>();
        }
    }
}