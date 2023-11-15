namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ParameterExpression
    /// </summary>
    public class ParameterExpression : ITypedSqlExpression
    {
        private readonly Func<string> _nameProducer;

        /// <summary> .cctor </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="type">Type</param>
        public ParameterExpression(TranslationContext context, Type type)
        {
            _nameProducer = context.NextLambdaParameterName();
            Type = type;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => _nameProducer();

        /// <inheritdoc />
        public Type Type { get; }
    }
}