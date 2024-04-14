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
        /// <param name="member">Member info</param>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        public ColumnExpression(
            MemberInfo member,
            Type type,
            ISqlExpression source)
        {
            if (source is not ParameterExpression)
            {
                throw new ArgumentException($"{nameof(ColumnExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Member = member;
            Type = type;
            Source = source;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => Member.Name;

        /// <summary>
        /// Member
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}