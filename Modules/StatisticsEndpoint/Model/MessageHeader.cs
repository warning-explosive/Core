namespace SpaceEngineers.Core.StatisticsEndpoint.Model
{
    using System;
    using GenericDomain;

    internal class MessageHeader : EntityBase
    {
        public MessageHeader(string key, string value, Type? valueType)
        {
            Key = key;
            Value = value;
            ValueType = valueType?.FullName;
        }

        public string Key { get; }

        public string Value { get; }

        public string? ValueType { get; }
    }
}