namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// NewExpression
    /// </summary>
    public class NewExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="parameters">Parameters</param>
        public NewExpression(Type itemType, IReadOnlyCollection<ISqlExpression> parameters)
        {
            ItemType = itemType;
            Parameters = parameters;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Parameters { get; }
    }
}