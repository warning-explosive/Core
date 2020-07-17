namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using MongoDB.Driver;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Persistence settings
    /// </summary>
    public class PersistenceSettings : IYamlSettings
    {
        /// <summary> .cctor </summary>
        /// <param name="mongoClientSettings">MongoClientSettings</param>
        public PersistenceSettings(MongoClientSettings mongoClientSettings)
        {
            MongoClientSettings = mongoClientSettings;
        }

        /// <summary>
        /// Mongo-client settings
        /// </summary>
        public MongoClientSettings MongoClientSettings { get; }
    }
}