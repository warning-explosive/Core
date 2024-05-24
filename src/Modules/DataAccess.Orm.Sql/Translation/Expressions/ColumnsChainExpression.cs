namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// ColumnsChainExpression
    /// </summary>
    public class ColumnsChainExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="members">MemberInfo</param>
        /// <param name="source">Source</param>
        public ColumnsChainExpression(
            Type type,
            IReadOnlyCollection<MemberInfo> members,
            ISqlExpression source)
        {
            if (source is not ParameterExpression)
            {
                throw new ArgumentException($"{nameof(ColumnsChainExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Type = type;
            Members = members;
            Source = source;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Members
        /// </summary>
        public IReadOnlyCollection<MemberInfo> Members { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}