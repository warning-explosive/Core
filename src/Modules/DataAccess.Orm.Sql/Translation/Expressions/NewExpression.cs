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
        /// <param name="type">Type</param>
        /// <param name="parameters">Parameters</param>
        public NewExpression(Type type, IReadOnlyCollection<ISqlExpression> parameters)
        {
            Type = type;
            Parameters = parameters;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Parameters { get; }
    }
}