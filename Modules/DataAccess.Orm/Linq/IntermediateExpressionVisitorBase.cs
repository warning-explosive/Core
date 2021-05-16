namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Linq;
    using Abstractions;
    using ValueObjects;

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
                FilterExpression filterExpression => VisitFilter(filterExpression),
                ProjectionExpression projectionExpression => VisitProjection(projectionExpression),
                NamedBindingExpression namedBindingExpression => VisitNamedBinding(namedBindingExpression),
                ParameterExpression parameterExpression => VisitParameter(parameterExpression),
                SimpleBindingExpression simpleBindingExpression => VisitSimpleBinding(simpleBindingExpression),
                NamedSourceExpression namedSourceExpression => VisitNamedSource(namedSourceExpression),
                NewExpression newExpression => VisitNew(newExpression),
                QuerySourceExpression querySourceExpression => VisitQuerySource(querySourceExpression),
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
                binaryExpression.ItemType,
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
                conditionalExpression.ItemType,
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
        /// Visit FilterExpression
        /// </summary>
        /// <param name="filterExpression">FilterExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitFilter(FilterExpression filterExpression)
        {
            return new FilterExpression(
                filterExpression.ItemType,
                Visit(filterExpression.Source),
                Visit(filterExpression.Expression));
        }

        /// <summary>
        /// Visit NamedBindingExpression
        /// </summary>
        /// <param name="namedBindingExpression">NamedBindingExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitNamedBinding(NamedBindingExpression namedBindingExpression)
        {
            return new NamedBindingExpression(Visit(namedBindingExpression.Expression), namedBindingExpression.Name);
        }

        /// <summary>
        /// Visit SimpleBindingExpression
        /// </summary>
        /// <param name="simpleBindingExpression">SimpleBindingExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            return new SimpleBindingExpression(
                simpleBindingExpression.ItemType,
                simpleBindingExpression.Name,
                Visit(simpleBindingExpression.Source));
        }

        /// <summary>
        /// Visit NamedSourceExpression
        /// </summary>
        /// <param name="namedSourceExpression">NamedSourceExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitNamedSource(NamedSourceExpression namedSourceExpression)
        {
            return new NamedSourceExpression(
                namedSourceExpression.ItemType,
                Visit(namedSourceExpression.Source),
                VisitParameter(namedSourceExpression.Parameter));
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
        protected virtual ParameterExpression VisitParameter(ParameterExpression parameterExpression)
        {
            return parameterExpression;
        }

        /// <summary>
        /// Visit ProjectionExpression
        /// </summary>
        /// <param name="projectionExpression">ProjectionExpression</param>
        /// <returns>Visited result</returns>
        protected virtual IIntermediateExpression VisitProjection(ProjectionExpression projectionExpression)
        {
            return new ProjectionExpression(
                projectionExpression.ItemType,
                Visit(projectionExpression.Source),
                projectionExpression.Bindings.Select(Visit));
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
    }
}