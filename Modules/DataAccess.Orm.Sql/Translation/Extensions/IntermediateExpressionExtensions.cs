namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using Expressions;
    using ParameterExpression = Expressions.ParameterExpression;

    /// <summary>
    /// IntermediateExpressionExtensions
    /// </summary>
    public static class IntermediateExpressionExtensions
    {
        /// <summary>
        /// Translates IIntermediateExpression to sql command
        /// </summary>
        /// <param name="expression">IIntermediateExpression</param>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="depth">Format depth</param>
        /// <returns>Sql command</returns>
        public static string Translate(
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

        /// <summary>
        /// Extracts ParameterExpressions
        /// </summary>
        /// <param name="expression">IIntermediateExpression</param>
        /// <returns>ParameterExpressions</returns>
        public static IReadOnlyDictionary<int, ParameterExpression> ExtractParameters(
            this IIntermediateExpression expression)
        {
            var extractor = new ExtractParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor.Parameters;
        }

        /// <summary>
        /// Compacts IIntermediateExpression
        /// </summary>
        /// <param name="expression">IIntermediateExpression</param>
        /// <param name="projection">ProjectionExpression</param>
        /// <returns>Compacted IIntermediateExpression</returns>
        public static IIntermediateExpression CompactExpression(
            this IIntermediateExpression expression,
            ProjectionExpression projection)
        {
            return new CompactExpressionVisitor(projection).Visit(expression);
        }

        /// <summary>
        /// Replaces join bindings
        /// </summary>
        /// <param name="expression">IIntermediateExpression</param>
        /// <param name="joinExpression">JoinExpression</param>
        /// <param name="applyNaming">Apply naming</param>
        /// <returns>IIntermediateExpression with replaced join bindings</returns>
        public static IIntermediateExpression ReplaceJoinBindings(
            this IIntermediateExpression expression,
            JoinExpression joinExpression,
            bool applyNaming)
        {
            return new ReplaceJoinBindingsVisitor(joinExpression, applyNaming).Visit(expression);
        }

        /// <summary>
        /// Replaces ParameterExpression
        /// </summary>
        /// <param name="expression">IIntermediateExpression</param>
        /// <param name="parameterExpression">ParameterExpression</param>
        /// <returns>IIntermediateExpression with replaced ParameterExpression</returns>
        public static IIntermediateExpression ReplaceParameter(
            this IIntermediateExpression expression,
            ParameterExpression parameterExpression)
        {
            return new ReplaceParameterVisitor(parameterExpression).Visit(expression);
        }

        /// <summary>
        /// Extracts members chain
        /// </summary>
        /// <param name="accessor">Accessor</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Members chain</returns>
        public static PropertyInfo[] ExtractMembersChain<TEntity>(this Expression<Func<TEntity, object?>> accessor)
        {
            var visitor = new ExtractMembersChainExpressionVisitor();
            _ = visitor.Visit(accessor);

            return visitor.Chain;
        }

        /// <summary>
        /// Extracts query parameters
        /// </summary>
        /// <param name="expression">IIntermediateExpression</param>
        /// <returns>Query parameters</returns>
        public static IReadOnlyDictionary<string, object?> ExtractQueryParameters(
            this IIntermediateExpression expression)
        {
            return expression
               .ExtractQueryParameterExpressions()
               .ToDictionary(
                    parameter => parameter.Name,
                    parameter => parameter.Value,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static IReadOnlyCollection<QueryParameterExpression> ExtractQueryParameterExpressions(
            this IIntermediateExpression expression)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor.QueryParameters;
        }
    }
}