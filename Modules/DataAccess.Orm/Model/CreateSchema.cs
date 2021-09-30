namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    /// <summary>
    /// Create schema change
    /// </summary>
    public class CreateSchema : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Schema name</param>
        public CreateSchema(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Schema name
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateSchema)} {Name}";
        }
    }
}