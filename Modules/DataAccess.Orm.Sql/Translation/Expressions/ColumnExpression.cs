namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Reflection;

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
        public string Name => Member.Name;

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression? Source { get; private set; }

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
                throw new InvalidOperationException("Source expression has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}