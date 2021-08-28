namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Dynamic;
    using Dynamic.Abstractions;
    using Expressions;
    using Visitors;

    internal static class IntermediateExpressionExtensions
    {
        internal static IReadOnlyDictionary<string, object?> GetQueryParametersValues(this object? obj)
        {
            return obj?.GetType().IsPrimitive() == true
                ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { [TranslationContext.QueryParameterFormat.Format(0)] = obj }
                : obj?.ToPropertyDictionary() ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        internal static object? ExtractQueryParameters(this IIntermediateExpression expression, IDependencyContainer dependencyContainer)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);

            if (!extractor.QueryParameters.Any())
            {
                return null;
            }

            var dynamicPropertyValues = extractor
                .QueryParameters
                .ToDictionary(parameter => new DynamicProperty(parameter.Type, parameter.Name),
                    parameter => parameter.Value);

            var dynamicClass = new DynamicClass().HasProperties(dynamicPropertyValues.Keys.ToArray());

            return dependencyContainer
                .Resolve<IDynamicClassProvider>()
                .CreateInstance(dynamicClass, dynamicPropertyValues);
        }

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