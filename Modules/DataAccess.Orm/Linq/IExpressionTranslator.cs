namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq.Expressions;

    /// <summary>
    /// IExpressionTranslator
    /// </summary>
    public interface IExpressionTranslator
    {
        /// <summary>
        /// Translates linq expression into DB command
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <returns>ICommand</returns>
        ICommand Translate(Expression expression);
    }
}