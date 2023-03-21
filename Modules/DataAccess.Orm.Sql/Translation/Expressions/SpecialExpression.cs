namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    /// <summary>
    /// SpecialExpression
    /// </summary>
    public class SpecialExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="text">Text</param>
        public SpecialExpression(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; }
    }
}