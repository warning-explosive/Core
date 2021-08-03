namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Reflection;
    using Internals;

    /// <summary>
    /// MemberTranslationContext
    /// </summary>
    public class MemberTranslationContext : TranslationContext
    {
        internal MemberTranslationContext(MemberInfo member, TranslationExpressionVisitor visitor)
            : base(visitor)
        {
            Member = member;
        }

        /// <summary>
        /// Member info
        /// </summary>
        public MemberInfo Member { get; }
    }
}