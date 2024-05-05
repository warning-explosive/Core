namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// MethodCallExpression
    /// </summary>
    public class MethodCallExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        /// <param name="arguments">Arguments</param>
        public MethodCallExpression(
            Type type,
            string name,
            ISqlExpression? source,
            IReadOnlyCollection<ISqlExpression> arguments)
        {
            if (source is not null
                && source is not BinaryExpression
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
                throw new ArgumentException($"{nameof(MethodCallExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Type = type;
            Name = name;
            Source = source;
            Arguments = arguments;
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
        /// Source expression
        /// </summary>
        public ISqlExpression? Source { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Arguments { get; }
    }
}