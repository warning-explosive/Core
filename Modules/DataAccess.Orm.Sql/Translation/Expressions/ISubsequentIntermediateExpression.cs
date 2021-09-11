namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    /// <summary>
    /// ISubsequentIntermediateExpression
    /// </summary>
    public interface ISubsequentIntermediateExpression : IIntermediateExpression
    {
        /// <summary>
        /// Source expression
        /// </summary>
        public IIntermediateExpression Source { get; }
    }
}