namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// UpdateExpression
    /// </summary>
    public class UpdateExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        public UpdateExpression(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }
    }
}