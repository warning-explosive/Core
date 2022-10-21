namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Concurrent;
    using Api.Abstractions;
    using Basics;

    internal class HostStartupActionsRegistry : IHostStartupActionsRegistry
    {
        private readonly ConcurrentDictionary<IHostStartupAction, object?> _registry =
            new ConcurrentDictionary<IHostStartupAction, object?>();

        public void Enroll(IHostStartupAction action)
        {
            _registry.Add(action, default);
        }

        public bool Contains(IHostStartupAction action)
        {
            return _registry.ContainsKey(action);
        }
    }
}