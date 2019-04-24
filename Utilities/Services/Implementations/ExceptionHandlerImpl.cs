namespace SpaceEngineers.Core.Utilities.Services.Implementations
{
    using System;
    using System.Linq;
    using Attributes;
    using Enumerations;
    using Interfaces;

    /// <inheritdoc />
    [Lifestyle(lifestyle: EnLifestyle.Singleton)]
    class ExceptionHandlerImpl : IExceptionHandler
    {
        private static readonly Type[] _exceptionTypesForSkip =
        {
            typeof(StackOverflowException),
            typeof(OutOfMemoryException),
            typeof(OperationCanceledException),
        };

        /// <inheritdoc />
        public void TryCatchFinally(Action action,
                                    Action<Exception> exceptionHandlerAction,
                                    Action finallyAction = null)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex) when(CanBeCatched(ex))
            {
                exceptionHandlerAction.Invoke(ex);
            }
            finally
            {
                finallyAction?.Invoke();
            }
        }

        /// <inheritdoc />
        public TReturn TryCatchFinally<TReturn>(Func<TReturn> action,
                                                Action<Exception> exceptionHandlerAction,
                                                Action finallyAction = null)
        {
            TReturn result = default;
            
            try
            {
                result = action.Invoke();
            }
            catch (Exception ex) when(CanBeCatched(ex))
            {
                exceptionHandlerAction.Invoke(ex);
            }
            finally
            {
                finallyAction?.Invoke();
            }

            return result;
        }

        private static bool CanBeCatched(Exception exception) => !_exceptionTypesForSkip.Contains(exception.GetType());
    }
}