namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Expressions;

    /// <summary>
    /// IMemberInfoTranslator
    /// </summary>
    public interface IMemberInfoTranslator
    {
        /// <summary>
        /// Recognize sql function
        /// </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="member">MemberInfo</param>
        /// <param name="expression">Expression</param>
        /// <returns>Recognition result</returns>
        bool TryRecognize(
            TranslationContext context,
            MemberInfo member,
            [NotNullWhen(true)] out ISqlExpression? expression);
    }
}