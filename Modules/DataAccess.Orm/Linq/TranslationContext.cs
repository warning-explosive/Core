namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using Internals;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext
    {
        /// <summary> .cctor </summary>
        /// <param name="visitor">TranslationExpressionVisitor</param>
        internal TranslationContext(TranslationExpressionVisitor visitor)
        {
            Visitor = visitor;
        }

        /// <summary>
        /// TranslationExpressionVisitor
        /// </summary>
        internal TranslationExpressionVisitor Visitor { get; }

        /// <summary>
        /// Gets next query parameter name
        /// </summary>
        /// <returns>Query parameter name</returns>
        public string NextQueryParameterName()
        {
            return Visitor.NextQueryParameterName();
        }
    }
}