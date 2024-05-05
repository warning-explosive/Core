namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// ColumnExpression
    /// </summary>
    public class ColumnExpression : ISqlExpression
    {
        private readonly string? _nameOverride;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="member">MemberInfo</param>
        /// <param name="source">Source</param>
        /// <param name="nameOverride">nameOverride</param>
        public ColumnExpression(
            Type type,
            MemberInfo member,
            ISqlExpression source,
            string? nameOverride = null)
        {
            if (source is not ParameterExpression)
            {
                throw new ArgumentException($"{nameof(ColumnExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            _nameOverride = nameOverride;

            Member = member;
            Type = type;
            Source = source;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => _nameOverride ?? Member.Name;

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