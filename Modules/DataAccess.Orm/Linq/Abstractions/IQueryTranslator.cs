namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    public interface IQueryTranslator : IResolvable
    {
        /// <summary>
        /// Translates linq expression to DB query
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query</returns>
        Task<IQuery> Translate(Expression expression, CancellationToken token);
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
        /// <param name="token">Cancellation token</param>
        /// <returns>Query</returns>
        Task<IQuery> Translate(TExpression intermediateExpression, CancellationToken token);
    }
}