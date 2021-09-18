namespace SpaceEngineers.Core.Basics
{
    using System;
    using Primitives;

    /// <summary>
    /// State extensions
    /// </summary>
    public static class StateExtensions
    {
        private const string ExclusiveOperationStarted = nameof(ExclusiveOperationStarted);

        /// <summary>
        /// Starts exclusive operation
        /// Ensures that operation was not started before and starts new one
        /// On dispose ensures that operation was started before and finishes it
        /// </summary>
        /// <param name="state">State</param>
        /// <returns>Disposable object</returns>
        public static IDisposable StartExclusiveOperation(this State state)
        {
            return Disposable.Create(state, Start, Finish);

            static void Start(State state)
            {
                state.Exchange<int>(ExclusiveOperationStarted,
                    original =>
                    {
                        if (original != default)
                        {
                            throw new InvalidOperationException("Operation has already been started");
                        }

                        return int.MaxValue;
                    });
            }

            static void Finish(State state)
            {
                state.Exchange<int>(ExclusiveOperationStarted,
                    original =>
                    {
                        if (original == default)
                        {
                            throw new InvalidOperationException("Operation wasn't stated");
                        }

                        return default;
                    });
            }
        }
    }
}