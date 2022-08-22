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
                UnaryExpression unaryExpression => VisitUnary(unaryExpression),
                ConditionalExpression conditionalExpression => VisitConditional(conditionalExpression),
                ConstantExpression constantExpression => VisitConstant(constantExpression),
                NamedBindingExpression namedBindingExpression => VisitNamedBinding(namedBindingExpression),
                SimpleBindingExpression simpleBindingExpression => VisitSimpleBinding(simpleBindingExpression),
                FilterExpression filterExpression => VisitFilter(filterExpression),
                NamedSourceExpression namedSourceExpression => VisitNamedSource(namedSourceExpression),
                ProjectionExpression projectionExpression => VisitProjection(projectionExpression),
                MethodCallExpression methodCallExpression => VisitMethodCall(methodCallExpression),
                NewExpression newExpression => VisitNew(newExpression),
                OrderByExpression orderByExpression => VisitOrderBy(orderByExpression),
                OrderByBindingExpression orderByBindingExpression => VisitOrderByBinding(orderByBindingExpression),
                ParameterExpression parameterExpression => VisitParameter(parameterExpression),
                QueryParameterExpression queryParameterExpression => VisitQueryParameter(queryParameterExpression),
                QuerySourceExpression querySourceExpression => VisitQuerySource(querySourceExpression),
                JoinExpression joinExpression => VisitJoinExpression(joinExpression),
                GroupByExpression groupByExpression => VisitGroupByExpression(groupByExpression),
                RowsFetchLimitExpression rowsFetchLimitExpression => VisitRowsFetchLimitExpression(rowsFetchLimitExpression),
                SpecialExpression specialExpression => VisitSpecialExpression(specialExpression),
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
        /// Visit UnaryExpression
        /// </summary>
        /// <param name="unaryExpression">UnaryExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitUnary(UnaryExpression unaryExpression)
        {
            return new UnaryExpression(
                unaryExpression.Type,
                unaryExpression.Operator,
                Visit(unaryExpression.Source));
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
                namedBindingExpression.Name,
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
                Visit(filterExpression.Predicate));
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
        /// Visit OrderByExpression
        /// </summary>
        /// <param name="orderByExpression">OrderByExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitOrderBy(OrderByExpression orderByExpression)
        {
            return new OrderByExpression(
                orderByExpression.Type,
                Visit(orderByExpression.Source),
                orderByExpression.Bindings.Select(Visit).ToList());
        }

        /// <summary>
        /// Visit OrderByBindingExpression
        /// </summary>
        /// <param name="orderByBindingExpression">OrderByBindingExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitOrderByBinding(OrderByBindingExpression orderByBindingExpression)
        {
            return new OrderByBindingExpression(
                Visit(orderByBindingExpression.Binding),
                orderByBindingExpression.OrderingDirection);
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
        /// Visit JoinExpression
        /// </summary>
        /// <param name="joinExpression">JoinExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitJoinExpression(JoinExpression joinExpression)
        {
            return new JoinExpression(
                Visit(joinExpression.LeftSource),
                Visit(joinExpression.RightSource),
                Visit(joinExpression.On));
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

        /// <summary>
        /// Visit RowsFetchLimitExpression
        /// </summary>
        /// <param name="rowsFetchLimitExpression">RowsFetchLimitExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitRowsFetchLimitExpression(RowsFetchLimitExpression rowsFetchLimitExpression)
        {
            return new RowsFetchLimitExpression(
                rowsFetchLimitExpression.Type,
                rowsFetchLimitExpression.RowsFetchLimit,
                Visit(rowsFetchLimitExpression.Source));
        }

        /// <summary>
        /// Visit SpecialExpression
        /// </summary>
        /// <param name="specialExpression">SpecialExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitSpecialExpression(SpecialExpression specialExpression)
        {
            return specialExpression;
        }
    }
}