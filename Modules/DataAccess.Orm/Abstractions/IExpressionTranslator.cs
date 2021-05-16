namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    using AutoWiring.Api.Abstractions;

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