namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// ISqlExpressionProvider
    /// </summary>
    public interface ISqlExpressionProvider : ICollectionResolvable<ISqlExpressionProvider>
    {
        /// <summary>
        /// Recognize sql function
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="expression">Expression</param>
        /// <returns>Recognition result</returns>
        bool TryRecognize(MemberInfo member, [NotNullWhen(true)] out IIntermediateExpression? expression);
    }
}