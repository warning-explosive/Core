namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Reflection;
    using Internals;

    /// <summary>
    /// MemberTranslationContext
    /// </summary>
    public class MemberTranslationContext
    {
        private readonly TranslationExpressionVisitor _visitor;

        internal MemberTranslationContext(MemberInfo member, TranslationExpressionVisitor visitor)
        {
            Member = member;
            _visitor = visitor;
        }

        /// <summary>
        /// Member info
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets next query parameter name
        /// </summary>
        /// <returns>Query parameter name</returns>
        public string NextQueryParameterName()
        {
            return _visitor.NextQueryParameterName();
        }
    }
}