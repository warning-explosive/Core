namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    /// <summary>
    /// IBindingSqlExpression
    /// </summary>
    public interface IBindingSqlExpression : ISqlExpression
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }
    }
}