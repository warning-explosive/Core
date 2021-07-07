namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    using System.Linq.Expressions;

    /// <summary>
    /// IQueryVisitor
    /// </summary>
    public interface IQueryVisitor
    {
        /// <summary>
        /// Apply the query visitor
        /// </summary>
        /// <param name="expression">Source expression</param>
        /// <returns>Visited expression</returns>
        Expression Apply(Expression expression);
    }
}