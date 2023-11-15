namespace SpaceEngineers.Core.GenericHost
{
    using System.Collections.Concurrent;
    using Basics;

    /// <summary>
    /// HostedServiceRegistry
    /// </summary>
    public class HostedServiceRegistry : IHostedServiceRegistry
    {
        private readonly ConcurrentDictionary<object, object?> _registry =
            new ConcurrentDictionary<object, object?>();

        /// <inheritdoc />
        public void Enroll(object obj)
        {
            _registry.Add(obj, default);
        }

        /// <inheritdoc />
        public bool Contains(object obj)
        {
            return _registry.ContainsKey(obj);
        }
    }
}