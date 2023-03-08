namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// NewExpression
    /// </summary>
    public class NewExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        public NewExpression(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }
    }
}