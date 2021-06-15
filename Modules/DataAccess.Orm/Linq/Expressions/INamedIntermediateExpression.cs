namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using Abstractions;

    /// <summary>
    /// INamedIntermediateExpression
    /// </summary>
    public interface INamedIntermediateExpression : IIntermediateExpression
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IIntermediateExpression Expression { get; }
    }
}