namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// CreateDatabase
    /// </summary>
    public class CreateDatabase : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="database">Database</param>
        public CreateDatabase(string database)
        {
            Database = database;
        }

        /// <summary>
        /// Database
        /// </summary>
        public string Database { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateDatabase)} {Database}";
        }
    }
}