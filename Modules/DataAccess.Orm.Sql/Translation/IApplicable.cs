namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using Expressions;

    /// <summary>
    /// IApplicable
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface IApplicable<TExpression>
        where TExpression : class, IIntermediateExpression
    {
        /// <summary>
        /// Applies expression to outer expression
        /// </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="expression">Expression to apply</param>
        void Apply(TranslationContext context, TExpression expression);
    }
}