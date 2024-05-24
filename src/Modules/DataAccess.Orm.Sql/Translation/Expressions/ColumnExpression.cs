namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// ColumnExpression
    /// </summary>
    public class ColumnExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="member">MemberInfo</param>
        /// <param name="source">Source</param>
        public ColumnExpression(
            Type type,
            MemberInfo member,
            ISqlExpression source)
        {
            if (source is not ParameterExpression)
            {
                throw new ArgumentException($"{nameof(ColumnExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Type = type;
            Member = member;
            Source = source;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Member
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}