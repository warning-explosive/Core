namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System.Linq.Expressions;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    public interface IQueryTranslator : IResolvable
    {
        /// <summary>
        /// Translates linq expression to DB query
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <returns>Query</returns>
        IQuery Translate(Expression expression);
    }

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface IQueryTranslator<in TExpression> : IResolvable
        where TExpression : IIntermediateExpression
    {
        /// <summary>
        /// Translates intermediate expression to DB query
        /// </summary>
        /// <param name="intermediateExpression">Intermediate expression</param>
        /// <returns>Query</returns>
        IQuery Translate(TExpression intermediateExpression);
    }
}