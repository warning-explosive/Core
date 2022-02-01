namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    /// <summary>
    /// IBindingIntermediateExpression
    /// </summary>
    public interface IBindingIntermediateExpression : IIntermediateExpression
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public IIntermediateExpression Source { get; }
    }
}