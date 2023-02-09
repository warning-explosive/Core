namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using Expressions;

    /// <summary>
    /// SqlExpressionVisitorBase
    /// </summary>
    public class SqlExpressionVisitorBase : ISqlExpressionVisitor
    {
        /// <inheritdoc />
        public ISqlExpression Visit(ISqlExpression expression)
        {
            return expression switch
            {
                BinaryExpression binaryExpression => VisitBinary(binaryExpression),
                UnaryExpression unaryExpression => VisitUnary(unaryExpression),
                ConditionalExpression conditionalExpression => VisitConditional(conditionalExpression),
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
        protected virtual ISqlExpression VisitBinary(BinaryExpression binaryExpression)
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
        protected virtual ISqlExpression VisitUnary(UnaryExpression unaryExpression)
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
        protected virtual ISqlExpression VisitConditional(ConditionalExpression conditionalExpression)
        {
            return new ConditionalExpression(
                conditionalExpression.Type,
                Visit(conditionalExpression.When),
                Visit(conditionalExpression.Then),
                Visit(conditionalExpression.Else));
        }

        /// <summary>
        /// Visit NamedBindingExpression
        /// </summary>
        /// <param name="namedBindingExpression">NamedBindingExpression</param>
        /// <returns>Visited result</returns>
        protected virtual ISqlExpression VisitNamedBinding(NamedBindingExpression namedBindingExpression)
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
        protected virtual ISqlExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
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
        protected virtual ISqlExpression VisitFilter(FilterExpression filterExpression)
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
        protected virtual ISqlExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
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
        protected virtual ISqlExpression VisitProjection(ProjectionExpression projectionExpression)
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
        protected virtual ISqlExpression VisitMethodCall(MethodCallExpression methodCallExpression)
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
        protected virtual ISqlExpression VisitNew(NewExpression newExpression)
        {
            return newExpression;
        }

        /// <summary>
        /// Visit OrderByExpression
        /// </summary>
        /// <param name="orderByExpression">OrderByExpression</param>
        /// <returns>Visited result</returns>
        protected virtual ISqlExpression VisitOrderBy(OrderByExpression orderByExpression)
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
        protected virtual ISqlExpression VisitOrderByBinding(OrderByBindingExpression orderByBindingExpression)
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
        protected virtual ISqlExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return parameterExpression;
        }

        /// <summary>
        /// Visit QueryParameterExpression
        /// </summary>
        /// <param name="queryParameterExpression">QueryParameterExpression</param>
        /// <returns>Visited result</returns>
        protected virtual ISqlExpression VisitQueryParameter(QueryParameterExpression queryParameterExpression)
        {
            return queryParameterExpression;
        }

        /// <summary>
        /// Visit QuerySourceExpression
        /// </summary>
        /// <param name="querySourceExpression">QuerySourceExpression</param>
        /// <returns>Visited result</returns>
        protected virtual ISqlExpression VisitQuerySource(QuerySourceExpression querySourceExpression)
        {
            return querySourceExpression;
        }

        /// <summary>
        /// Visit JoinExpression
        /// </summary>
        /// <param name="joinExpression">JoinExpression</param>
        /// <returns>Visited result</returns>
        protected virtual ISqlExpression VisitJoinExpression(JoinExpression joinExpression)
        {
            return new JoinExpression(
                Visit(joinExpression.LeftSource),
                Visit(joinExpression.RightSource),
                Visit(joinExpression.On));
        }

        /// <summary>
        /// Visit RowsFetchLimitExpression
        /// </summary>
        /// <param name="rowsFetchLimitExpression">RowsFetchLimitExpression</param>
        /// <returns>Visited result</returns>
        protected virtual ISqlExpression VisitRowsFetchLimitExpression(RowsFetchLimitExpression rowsFetchLimitExpression)
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
        protected virtual ISqlExpression VisitSpecialExpression(SpecialExpression specialExpression)
        {
            return specialExpression;
        }
    }
}