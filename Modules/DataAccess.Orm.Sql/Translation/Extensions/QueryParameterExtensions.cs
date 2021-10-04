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
        internal static IReadOnlyDictionary<string, (Type, object?)> AsQueryParametersValues(this object? obj)
        {
            if (obj?.GetType().IsPrimitive() == true)
            {
                return new Dictionary<string, (Type, object?)>(StringComparer.OrdinalIgnoreCase)
                {
                    [TranslationContext.QueryParameterFormat.Format(0)] = (obj.GetType(), obj)
                };
            }
            else
            {
                return obj?.ToPropertyDictionary()
                       ?? new Dictionary<string, (Type, object?)>(StringComparer.OrdinalIgnoreCase);
            }
        }

        internal static IReadOnlyDictionary<string, (Type, object?)> ExtractQueryParameters(this IIntermediateExpression expression)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);

            if (!extractor.QueryParameters.Any())
            {
                return new Dictionary<string, (Type, object?)>();
            }

            return extractor
                .QueryParameters
                .ToDictionary(parameter => parameter.Name,
                    parameter => (parameter.Type, parameter.Value),
                    StringComparer.OrdinalIgnoreCase);
        }

        internal static string QueryParameterSqlExpression(this object? value, Type type, IDependencyContainer dependencyContainer)
        {
            return dependencyContainer
                .ResolveGeneric(typeof(IQueryParameterTranslator<>), type)
                .CallMethod(nameof(IQueryParameterTranslator<object>.Translate))
                .WithArgument(value)
                .Invoke<string>();
        }
    }
}