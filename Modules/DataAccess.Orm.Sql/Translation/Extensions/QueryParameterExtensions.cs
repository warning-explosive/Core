namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using Basics;
    using CompositionRoot;

    /// <summary>
    /// QueryParameterExtensions
    /// </summary>
    public static class QueryParameterExtensions
    {
        /// <summary>
        /// AsQueryParametersValues
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Query parameters values</returns>
        public static IReadOnlyDictionary<string, object?> AsQueryParametersValues(this object? obj)
        {
            return obj?.GetType().IsPrimitive() == true
                ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { [TranslationContext.QueryParameterFormat.Format(0)] = obj }
                : obj?.ToPropertyDictionary() ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Translates value to QueryParameterSqlExpression
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <returns>QueryParameterSqlExpression</returns>
        public static string QueryParameterSqlExpression(this object? value, IDependencyContainer dependencyContainer)
        {
            // TODO: #143 - ResolveGeneric
            return dependencyContainer
                .ResolveGeneric(typeof(IQueryParameterTranslator<>), value?.GetType() ?? typeof(object))
                .CallMethod(nameof(IQueryParameterTranslator<object>.Translate))
                .WithArgument(value)
                .Invoke<string>();
        }
    }
}