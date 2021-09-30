namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Expressions;

    internal static class IntermediateExpressionExtensions
    {
        internal static IEnumerable<IBindingIntermediateExpression> SelectAll(
            this Type type,
            ParameterExpression parameter)
        {
            if (type.IsClass)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);

                foreach (var property in properties)
                {
                    yield return new SimpleBindingExpression(property, property.PropertyType, parameter);
                }
            }
        }

        internal static Task<string> Translate(
            this IIntermediateExpression expression,
            IDependencyContainer dependencyContainer,
            int depth,
            CancellationToken token)
        {
            return typeof(IntermediateExpressionExtensions)
                .CallMethod(nameof(Translate))
                .WithTypeArgument(expression.GetType())
                .WithArguments(dependencyContainer, expression, depth, token)
                .Invoke<Task<string>>();
        }

        private static Task<string> Translate<TExpression>(
            IDependencyContainer dependencyContainer,
            TExpression expression,
            int depth,
            CancellationToken token)
            where TExpression : IIntermediateExpression
        {
            return dependencyContainer
                .Resolve<IExpressionTranslator<TExpression>>()
                .Translate(expression, depth, token);
        }
    }
}