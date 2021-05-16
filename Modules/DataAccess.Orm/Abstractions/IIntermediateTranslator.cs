namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    using System.Linq.Expressions;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IIntermediateTranslator
    /// </summary>
    public interface IIntermediateTranslator : IResolvable
    {
        /// <summary> Translate </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Intermediate expression</returns>
        IIntermediateExpression Translate(Expression expression);
    }
}