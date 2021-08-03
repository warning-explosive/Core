namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Basics;
    using Dynamic.Api;
    using Dynamic.Api.Abstractions;
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

        internal static object? ExtractParameters(this IIntermediateExpression expression, IDependencyContainer dependencyContainer)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);

            if (!extractor.QueryParameters.Any())
            {
                return null;
            }

            var dynamicPropertyValues = extractor
                .QueryParameters
                .ToDictionary(parameter => new DynamicProperty(parameter.ItemType, parameter.Name),
                    parameter => parameter.Value);

            var dynamicClass = new DynamicClass().HasProperties(dynamicPropertyValues.Keys.ToArray());

            return dependencyContainer
                .Resolve<IDynamicClassProvider>()
                .CreateInstance(dynamicClass, dynamicPropertyValues);
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