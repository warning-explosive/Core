namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using Abstractions;

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