namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    /// <summary>
    /// IIntermediateExpressionVisitor
    /// </summary>
    public interface IIntermediateExpressionVisitor
    {
        /// <summary>
        /// Visit expression
        /// </summary>
        /// <param name="expression">Source expression</param>
        /// <returns>Modified expression</returns>
        IIntermediateExpression Visit(IIntermediateExpression expression);
    }
}