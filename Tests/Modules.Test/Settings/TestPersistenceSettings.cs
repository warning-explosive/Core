namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using CrossCuttingConcerns.Settings;
    using MongoDB.Driver;

    /// <summary>
    /// Test persistence settings
    /// </summary>
    public class TestPersistenceSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public TestPersistenceSettings()
        {
            MongoClientSettings = new MongoClientSettings();
        }

        /// <summary>
        /// Mongo-client settings
        /// </summary>
        public MongoClientSettings MongoClientSettings { get; set; }
    }
}