namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Expressions;

    internal static class QueryParameterExtensions
    {
        internal static IReadOnlyDictionary<string, object?> AsQueryParametersValues(this object? obj)
        {
            return obj?.GetType().IsPrimitive() == true
                ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    [TranslationContext.QueryParameterFormat.Format(0)] = obj
                }
                : obj?.ToPropertyDictionary()
                  ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        internal static IReadOnlyDictionary<string, object?> ExtractQueryParameters(this IIntermediateExpression expression)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);

            if (!extractor.QueryParameters.Any())
            {
                return new Dictionary<string, object?>();
            }

            return extractor
                .QueryParameters
                .ToDictionary(
                    parameter => parameter.Name,
                    parameter => parameter.Value,
                    StringComparer.OrdinalIgnoreCase);
        }

        internal static string QueryParameterSqlExpression(this object? value, IDependencyContainer dependencyContainer)
        {
            return dependencyContainer
                .ResolveGeneric(typeof(IQueryParameterTranslator<>), value?.GetType() ?? typeof(object))
                .CallMethod(nameof(IQueryParameterTranslator<object>.Translate))
                .WithArgument(value)
                .Invoke<string>();
        }
    }
}