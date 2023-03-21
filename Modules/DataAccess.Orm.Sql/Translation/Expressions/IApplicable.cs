namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    /// <summary>
    /// IApplicable
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface IApplicable<TExpression>
        where TExpression : class, ISqlExpression
    {
        /// <summary>
        /// Applies expression to outer expression
        /// </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="expression">Expression to apply</param>
        void Apply(TranslationContext context, TExpression expression);
    }
}