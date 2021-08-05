namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System.Reflection;
    using Abstractions;

    /// <summary>
    /// IBindingIntermediateExpression
    /// </summary>
    public interface IBindingIntermediateExpression : IIntermediateExpression
    {
        /// <summary>
        /// Member info
        /// </summary>
        MemberInfo Member { get; }

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