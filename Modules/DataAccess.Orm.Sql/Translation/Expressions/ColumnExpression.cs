namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// ColumnExpression
    /// </summary>
    public class ColumnExpression : ITypedSqlExpression,
                                    IApplicable<ColumnExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="member">Member info</param>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        public ColumnExpression(
            MemberInfo member,
            Type type,
            ISqlExpression? source = null)
        {
            Member = member;
            Type = type;
            Source = source;
        }

        /// <summary>
        /// Member
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return Flatten()
                   .Reverse()
                   .Select(expression => expression.Member.Name)
                   .ToString("_");
            }
        }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression? Source { get; private set; }

        /// <summary>
        /// Gets flat collection of underneath expressions
        /// </summary>
        /// <returns>Flat collection</returns>
        public IEnumerable<ISqlExpression> FlattenCompletely()
        {
            ISqlExpression? current = this;

            while (current != null)
            {
                yield return current;

                current = current is ColumnExpression columnExpression
                    ? columnExpression.Source
                    : current is RenameExpression renameExpression
                        ? renameExpression.Source
                        : null;
            }
        }

        /// <summary>
        /// Gets flat collection of underneath column expressions
        /// </summary>
        /// <returns>Flat collection</returns>
        public IEnumerable<ColumnExpression> Flatten()
        {
            var current = this;

            while (current != null)
            {
                yield return current;
                current = current.Source as ColumnExpression;
            }
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Column expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}