namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Linq;

    /// <summary>
    /// Flat query
    /// </summary>
    public sealed class FlatQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandParameters">Command parameters</param>
        public FlatQuery(string commandText, IReadOnlyDictionary<string, string> commandParameters)
        {
            CommandText = commandText;
            CommandParameters = commandParameters;
        }

        /// <summary>
        /// Command text
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// Command parameters
        /// </summary>
        public IReadOnlyDictionary<string, string> CommandParameters { get; }
    }
}