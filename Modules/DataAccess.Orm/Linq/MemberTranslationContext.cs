namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Reflection;

    /// <summary>
    /// MemberTranslationContext
    /// </summary>
    public class MemberTranslationContext : TranslationContext
    {
        internal MemberTranslationContext(TranslationContext context, MemberInfo member)
            : base(context)
        {
            Member = member;
        }

        /// <summary>
        /// Member info
        /// </summary>
        public MemberInfo Member { get; }
    }
}