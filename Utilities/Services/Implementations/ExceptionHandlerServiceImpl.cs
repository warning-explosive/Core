namespace SpaceEngineers.Core.Utilities.Services.Implementations
{
    using System;
    using System.Linq;
    using Interfaces;

    /// <inheritdoc />
    public class ExceptionHandlerServiceImpl : IExceptionHandlerService
    {
        private static readonly Type[] ExceptionTypesForSkip =
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

        private static bool CanBeCatched(Exception exception)
        {
            return !ExceptionTypesForSkip.Contains(exception.GetType());
        }
    }
}