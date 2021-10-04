namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
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