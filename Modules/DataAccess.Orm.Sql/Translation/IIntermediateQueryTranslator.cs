namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using AutoRegistration.Api.Abstractions;
    using Expressions;
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
        /// <returns>Query</returns>
        IQuery Translate(TExpression intermediateExpression);
    }
}