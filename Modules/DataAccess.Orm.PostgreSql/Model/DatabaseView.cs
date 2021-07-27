namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using Contract.Abstractions;
    using GenericDomain;

    internal class DatabaseView : EntityBase, IView
    {
        public DatabaseView(string name, string query)
        {
            Name = name;
            Query = query;
        }

        public string Name { get; private set; }

        public string Query { get; private set; }
    }
}