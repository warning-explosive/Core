namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using Expressions;

    /// <summary>
    /// IntermediateExpressionVisitorBase
    /// </summary>
    public class IntermediateExpressionVisitorBase : IIntermediateExpressionVisitor
    {
        /// <inheritdoc />
        public IIntermediateExpression Visit(IIntermediateExpression expression)
        {
            return expression switch
            {
                BinaryExpression binaryExpression => VisitBinary(binaryExpression),
                ConditionalExpression conditionalExpression => VisitConditional(conditionalExpression),
                ConstantExpression constantExpression => VisitConstant(constantExpression),
                NamedBindingExpression namedBindingExpression => VisitNamedBinding(namedBindingExpression),
                SimpleBindingExpression simpleBindingExpression => VisitSimpleBinding(simpleBindingExpression),
                FilterExpression filterExpression => VisitFilter(filterExpression),
                NamedSourceExpression namedSourceExpression => VisitNamedSource(namedSourceExpression),
                ProjectionExpression projectionExpression => VisitProjection(projectionExpression),
                MethodCallExpression methodCallExpression => VisitMethodCall(methodCallExpression),
                NewExpression newExpression => VisitNew(newExpression),
                ParameterExpression parameterExpression => VisitParameter(parameterExpression),
                QueryParameterExpression queryParameterExpression => VisitQueryParameter(queryParameterExpression),
                QuerySourceExpression querySourceExpression => VisitQuerySource(querySourceExpression),
                ColumnChainExpression columnChainExpression => VisitColumnChainExpression(columnChainExpression),
                GroupByExpression groupByExpression => VisitGroupByExpression(groupByExpression),
                _ => throw new NotSupportedException(expression.GetType().FullName)
            };
        }

        /// <summary>
        /// Visit BinaryExpression
        /// </summary>
        /// <param name="binaryExpression">BinaryExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitBinary(BinaryExpression binaryExpression)
        {
            return new BinaryExpression(
                binaryExpression.Type,
                binaryExpression.Operator,
                Visit(binaryExpression.Left),
                Visit(binaryExpression.Right));
        }

        /// <summary>
        /// Visit ConditionalExpression
        /// </summary>
        /// <param name="conditionalExpression">ConditionalExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitConditional(ConditionalExpression conditionalExpression)
        {
            return new ConditionalExpression(
                conditionalExpression.Type,
                Visit(conditionalExpression.When),
                Visit(conditionalExpression.Then),
                Visit(conditionalExpression.Else));
        }

        /// <summary>
        /// Visit ConstantExpression
        /// </summary>
        /// <param name="constantExpression">ConstantExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitConstant(ConstantExpression constantExpression)
        {
            return constantExpression;
        }

        /// <summary>
        /// Visit NamedBindingExpression
        /// </summary>
        /// <param name="namedBindingExpression">NamedBindingExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitNamedBinding(NamedBindingExpression namedBindingExpression)
        {
            return new NamedBindingExpression(
                namedBindingExpression.Member,
                Visit(namedBindingExpression.Source));
        }

        /// <summary>
        /// Visit SimpleBindingExpression
        /// </summary>
        /// <param name="simpleBindingExpression">SimpleBindingExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            return new SimpleBindingExpression(
                simpleBindingExpression.Member,
                simpleBindingExpression.Type,
                Visit(simpleBindingExpression.Source));
        }

        /// <summary>
        /// Visit FilterExpression
        /// </summary>
        /// <param name="filterExpression">FilterExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitFilter(FilterExpression filterExpression)
        {
            return new FilterExpression(
                filterExpression.Type,
                Visit(filterExpression.Source),
                Visit(filterExpression.Expression));
        }

        /// <summary>
        /// Visit NamedSourceExpression
        /// </summary>
        /// <param name="namedSourceExpression">NamedSourceExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
        {
            return new NamedSourceExpression(
                namedSourceExpression.Type,
                Visit(namedSourceExpression.Source),
                Visit(namedSourceExpression.Parameter));
        }

        /// <summary>
        /// Visit ProjectionExpression
        /// </summary>
        /// <param name="projectionExpression">ProjectionExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitProjection(ProjectionExpression projectionExpression)
        {
            return new ProjectionExpression(
                projectionExpression.Type,
                Visit(projectionExpression.Source),
                projectionExpression.Bindings.Select(Visit));
        }

        /// <summary>
        /// Visit MethodCallExpression
        /// </summary>
        /// <param name="methodCallExpression">MethodCallExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            return new MethodCallExpression(
                methodCallExpression.Type,
                methodCallExpression.Name,
                methodCallExpression.Source != null ? Visit(methodCallExpression.Source) : null,
                methodCallExpression.Arguments.Select(Visit).ToList());
        }

        /// <summary>
        /// Visit NewExpression
        /// </summary>
        /// <param name="newExpression">NewExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitNew(NewExpression newExpression)
        {
            return newExpression;
        }

        /// <summary>
        /// Visit ParameterExpression
        /// </summary>
        /// <param name="parameterExpression">ParameterExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return parameterExpression;
        }

        /// <summary>
        /// Visit QueryParameterExpression
        /// </summary>
        /// <param name="queryParameterExpression">QueryParameterExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitQueryParameter(QueryParameterExpression queryParameterExpression)
        {
            return queryParameterExpression;
        }

        /// <summary>
        /// Visit QuerySourceExpression
        /// </summary>
        /// <param name="querySourceExpression">QuerySourceExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitQuerySource(QuerySourceExpression querySourceExpression)
        {
            return querySourceExpression;
        }

        /// <summary>
        /// Visit ColumnChainExpression
        /// </summary>
        /// <param name="columnChainExpression">ColumnChainExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitColumnChainExpression(ColumnChainExpression columnChainExpression)
        {
            return columnChainExpression;
        }

        /// <summary>
        /// Visit GroupByExpression
        /// </summary>
        /// <param name="groupByExpression">GroupByExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitGroupByExpression(GroupByExpression groupByExpression)
        {
            return groupByExpression;
        }
    }
}