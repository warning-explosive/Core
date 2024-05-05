namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// ColumnsChainExpression
    /// </summary>
    public class ColumnsChainExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="members">MemberInfos</param>
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

            Members = members;
            Type = type;
            Source = source;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => Members.ToString("_", member => member.Name);

        /// <summary>
        /// Member
        /// </summary>
        public IReadOnlyCollection<MemberInfo> Members { get; }

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