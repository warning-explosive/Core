namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Expressions;

    internal static class IntermediateExpressionExtensions
    {
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

        internal static IReadOnlyDictionary<int, ParameterExpression> ExtractParameters(
            this IIntermediateExpression expression)
        {
            var extractor = new ExtractParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor.Parameters;
        }

        internal static IReadOnlyCollection<QueryParameterExpression> ExtractQueryParameterExpressions(
            this IIntermediateExpression expression)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor.QueryParameters;
        }

        internal static IReadOnlyDictionary<string, object?> ExtractQueryParameters(
            this IIntermediateExpression expression)
        {
            return expression
               .ExtractQueryParameterExpressions()
               .ToDictionary(
                    parameter => parameter.Name,
                    parameter => parameter.Value,
                    StringComparer.OrdinalIgnoreCase);
        }

        internal static IIntermediateExpression ReplaceFilterExpression(
            this IIntermediateExpression expression,
            ProjectionExpression projection)
        {
            return new ReplaceFilterExpressionVisitor(projection).Visit(expression);
        }

        internal static IIntermediateExpression ReplaceJoinBindings(
            this IIntermediateExpression expression,
            JoinExpression joinExpression,
            bool applyNaming)
        {
            return new ReplaceJoinBindingsVisitor(joinExpression, applyNaming).Visit(expression);
        }

        internal static IIntermediateExpression ReplaceParameter(
            this IIntermediateExpression expression,
            ParameterExpression parameterExpression)
        {
            return new ReplaceParameterVisitor(parameterExpression).Visit(expression);
        }
    }
}