namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using CrossCuttingConcerns.Settings;
    using MongoDB.Driver;

    /// <summary>
    /// Persistence settings
    /// </summary>
    public class PersistenceSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public PersistenceSettings()
        {
            MongoClientSettings = new MongoClientSettings();
        }

        /// <summary>
        /// Mongo-client settings
        /// </summary>
        public MongoClientSettings MongoClientSettings { get; set; }
    }
}