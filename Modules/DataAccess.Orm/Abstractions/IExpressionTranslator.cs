namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    using System.Linq.Expressions;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IExpressionTranslator
    /// </summary>
    public interface IExpressionTranslator : IResolvable
    {
        /// <summary> Translate </summary>
        /// <param name="expression">Expression</param>
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
        /// <summary> Translate </summary>
        /// <param name="expression">Expression</param>
        /// <param name="depth">Depth</param>
        /// <returns>Translated expression</returns>
        string Translate(TExpression expression, int depth);
    }
}