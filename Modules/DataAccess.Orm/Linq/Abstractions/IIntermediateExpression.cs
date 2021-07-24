namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System;

    /// <summary>
    /// IIntermediateExpression
    /// </summary>
    public interface IIntermediateExpression
    {
        /// <summary>
        /// Item type
        /// </summary>
        Type ItemType { get; }
    }
}