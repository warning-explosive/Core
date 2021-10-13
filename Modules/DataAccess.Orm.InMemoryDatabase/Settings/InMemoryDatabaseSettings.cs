namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Settings
{
    using System.Data;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;

    /// <summary>
    /// InMemoryDatabaseSettings
    /// </summary>
    public class InMemoryDatabaseSettings : IYamlSettings
    {
        private const string Format = "{0}={1}";

        /// <summary> .cctor </summary>
        public InMemoryDatabaseSettings()
        {
            Database = "SpaceEngineerDatabase";
            IsolationLevel = IsolationLevel.Snapshot;
        }

        /// <summary>
        /// Database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Isolation level
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this
                .ToPropertyDictionary()
                .ToString(";", pair => Format.Format(pair.Key, pair.Value.Value?.ToString() ?? "null"));
        }
    }
}