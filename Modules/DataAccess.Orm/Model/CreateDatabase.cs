namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// Create database change
    /// </summary>
    public class CreateDatabase : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Database name</param>
        public CreateDatabase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Database name
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateDatabase)} {Name}";
        }
    }
}