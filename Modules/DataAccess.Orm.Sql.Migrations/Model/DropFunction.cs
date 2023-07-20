namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    /// <summary>
    /// DropFunction
    /// </summary>
    public class DropFunction : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="function">Function</param>
        public DropFunction(
            string schema,
            string function)
        {
            Schema = schema;
            Function = function;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Function
        /// </summary>
        public string Function { get; }
    }
}