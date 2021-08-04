namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System;

    /// <summary>
    /// IIntermediateExpression
    /// </summary>
    public interface IIntermediateExpression
    {
        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }
    }
}