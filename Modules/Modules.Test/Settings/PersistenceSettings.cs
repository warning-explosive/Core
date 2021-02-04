namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using Core.SettingsManager.Abstractions;
    using MongoDB.Driver;

    /// <summary>
    /// Persistence settings
    /// </summary>
    public class PersistenceSettings : IJsonSettings
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