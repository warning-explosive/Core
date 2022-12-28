namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using Expressions;

    /// <summary>
    /// IExpressionTranslator
    /// </summary>
    public interface IExpressionTranslator
    {
        /// <summary>
        /// Translates linq expression to sql expression
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <returns>Sql expression</returns>
        ISqlExpression Translate(Expression expression);
    }
}