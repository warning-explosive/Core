namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.HttpApi
{
    internal class RabbitMqQueue
    {
        public RabbitMqQueue(string name)
        {
            Name = name;
        }

        public string Name { get; init; }
    }
}