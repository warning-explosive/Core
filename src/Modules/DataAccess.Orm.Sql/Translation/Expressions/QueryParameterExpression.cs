namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// QueryParameterExpression
    /// </summary>
    public class QueryParameterExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        public QueryParameterExpression(Type type, string name)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Query parameter name
        /// </summary>
        public string Name { get; }
    }
}