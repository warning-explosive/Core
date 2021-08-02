namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Basics;
    using Expressions;

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

        internal static IEnumerable<INamedIntermediateExpression> SelectAll(
            this Type type,
            ParameterExpression parameter)
        {
            if (type.IsClass)
            {
                var properties = type
                    .GetProperties(BindingFlags.Public
                                   | BindingFlags.NonPublic
                                   | BindingFlags.Instance
                                   | BindingFlags.GetProperty
                                   | BindingFlags.SetProperty);

                foreach (var property in properties)
                {
                    yield return new SimpleBindingExpression(property.PropertyType, property.Name, parameter);
                }
            }
        }
    }
}