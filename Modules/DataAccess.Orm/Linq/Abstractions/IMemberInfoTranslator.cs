namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// IMemberInfoTranslator
    /// </summary>
    public interface IMemberInfoTranslator
    {
        /// <summary>
        /// Recognize sql function
        /// </summary>
        /// <param name="context">MemberTranslationContext</param>
        /// <param name="expression">Expression</param>
        /// <returns>Recognition result</returns>
        bool TryRecognize(MemberTranslationContext context, [NotNullWhen(true)] out IIntermediateExpression? expression);
    }
}