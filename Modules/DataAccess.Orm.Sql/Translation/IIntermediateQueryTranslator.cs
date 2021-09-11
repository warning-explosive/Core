namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Orm.Linq;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface IIntermediateQueryTranslator<in TExpression> : IResolvable
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