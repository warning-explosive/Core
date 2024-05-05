namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// RenameExpression
    /// </summary>
    public class RenameExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source expression</param>
        public RenameExpression(
            Type type,
            string name,
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
            Name = name;
            Source = source;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}