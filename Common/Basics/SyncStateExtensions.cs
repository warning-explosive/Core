namespace SpaceEngineers.Core.Basics
{
    using System;
    using Primitives;

    /// <summary>
    /// State extensions
    /// </summary>
    public static class StateExtensions
    {
        /// <summary>
        /// Starts exclusive operation
        /// Ensures that operation was not started before and starts new one
        /// On dispose ensures that operation was started before and finishes it
        /// </summary>
        /// <param name="syncState">State</param>
        /// <param name="key">Key</param>
        /// <returns>Disposable object</returns>
        public static IDisposable StartExclusiveOperation(this SyncState syncState, string key)
        {
            return Disposable.Create((state: syncState, key), Start, Finish);

            static void Start((SyncState, string) state)
            {
                var (syncState, key) = state;

                syncState.Exchange<int, string>(
                    key,
                    key,
                    (original, context) =>
                    {
                        if (original != default)
                        {
                            throw new InvalidOperationException($"Operation '{context}' has already been started");
                        }

                        return int.MaxValue;
                    });
            }

            static void Finish((SyncState, string) state)
            {
                var (syncState, key) = state;

                syncState.Exchange<int, string>(
                    key,
                    key,
                    (original, context) =>
                    {
                        if (original == default)
                        {
                            throw new InvalidOperationException($"Operation '{context}' wasn't stated");
                        }

                        return default;
                    });
            }
        }
    }
}