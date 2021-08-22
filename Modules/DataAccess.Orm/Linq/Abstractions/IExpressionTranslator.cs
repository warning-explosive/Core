namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IExpressionTranslator
    /// </summary>
    public interface IExpressionTranslator : IResolvable
    {
        /// <summary>
        /// Translates linq expression to intermediate expression
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <returns>Intermediate expression</returns>
        IIntermediateExpression Translate(Expression expression);
    }

    /// <summary>
    /// IExpressionTranslator
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface IExpressionTranslator<TExpression> : IResolvable
        where TExpression : IIntermediateExpression
    {
        /// <summary>
        /// Translates intermediate expression into DB query
        /// </summary>
        /// <param name="expression">Intermediate expression</param>
        /// <param name="depth">Depth</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Translated expression</returns>
        Task<string> Translate(TExpression expression, int depth, CancellationToken token);
    }
}