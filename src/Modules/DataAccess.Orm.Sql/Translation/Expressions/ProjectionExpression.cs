namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using Basics;

    /// <summary>
    /// ProjectionExpression
    /// </summary>
    public class ProjectionExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="source">Source expression</param>
        /// <param name="expressions">Expressions</param>
        public ProjectionExpression(
            Type itemType,
            ISqlExpression source,
            IReadOnlyCollection<ISqlExpression> expressions)
        {
            if (source is not NamedSourceExpression)
            {
                throw new ArgumentException($"{nameof(ProjectionExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            ItemType = itemType;
            Source = source;
            IsProjectionToClass = itemType.IsClass && !itemType.IsPrimitive() && !itemType.IsCollection();
            IsAnonymousProjection = itemType.IsCompilerGenerated();
            Expressions = expressions;

            // todo: new expression
            /*IsProjectionToClass = true;
            IsAnonymousProjection = expression.Type.IsCompilerGenerated();*/

            // TODO: simplify to assigment
            /*if (Source is JoinExpression join)
            {
                expression = ReplaceJoinParameterExpressionsVisitor.Replace(expression, join);
            }

            if (expression is ParameterExpression)
            {
                return;
            }

            _expressions.Add(expression);*/
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Is projection creates anonymous or user defined class
        /// </summary>
        public bool IsProjectionToClass { get; }

        /// <summary>
        /// Is projection creates anonymous class
        /// </summary>
        public bool IsAnonymousProjection { get; }

        /// <summary>
        /// Is projection takes distinct values
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions { get; }
    }
}