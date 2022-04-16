namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using Basics;
    using CompositionRoot.Api.Abstractions;

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