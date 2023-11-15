namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.HttpApi
{
    internal class RabbitMqVirtualHost
    {
        public RabbitMqVirtualHost(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; init; }

        public string Description { get; init; }
    }
}