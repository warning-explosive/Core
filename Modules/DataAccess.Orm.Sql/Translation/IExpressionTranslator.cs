namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using Expressions;

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
        /// <returns>Translated expression</returns>
        string Translate(TExpression expression, int depth);
    }
}