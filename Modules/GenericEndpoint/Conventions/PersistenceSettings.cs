namespace SpaceEngineers.Core.GenericEndpoint.Conventions
{
    using System;
    using MongoDB.Driver;
    using SettingsManager;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Persistence settings
    /// </summary>
    public class PersistenceSettings : IYamlSettings
    {
        /// <summary>
        /// Mongo-client settings
        /// </summary>
        public MongoClientSettings MongoClientSettings { get; set; }
    }
}