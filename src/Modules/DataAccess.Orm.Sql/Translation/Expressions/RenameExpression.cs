namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// RenameExpression
    /// </summary>
    public class RenameExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="members">MemberInfo</param>
        /// <param name="source">Source expression</param>
        public RenameExpression(
            Type type,
            IReadOnlyCollection<MemberInfo> members,
            ISqlExpression source)
        {
            if (source is not BinaryExpression
                && source is not ColumnsChainExpression
                && source is not ColumnExpression
                && source is not ConditionalExpression
                && source is not JsonAttributeExpression
                && source is not MethodCallExpression
                && source is not NullExpression
                && source is not ParameterExpression
                && source is not ParenthesesExpression
                && source is not QueryParameterExpression
                && source is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(RenameExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
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