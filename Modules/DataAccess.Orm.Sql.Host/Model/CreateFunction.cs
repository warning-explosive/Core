namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// CreateFunction
    /// </summary>
    public class CreateFunction : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="function">Function</param>
        /// <param name="commandText">Command text</param>
        public CreateFunction(
            string schema,
            string function,
            string commandText)
        {
            Schema = schema;
            Function = function;
            CommandText = commandText;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Function
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// CommandText
        /// </summary>
        public string CommandText { get; }
    }
}