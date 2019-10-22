namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Enumerations;

    /// <inheritdoc />
    [Lifestyle(lifestyle: EnLifestyle.Singleton)]
    internal class ExceptionHandlerImpl : IExceptionHandler
    {
        private static readonly Type[] _exceptionTypesForSkip =
        {
            typeof(StackOverflowException),
            typeof(OutOfMemoryException),
            typeof(OperationCanceledException),
        };

        /// <inheritdoc />
        public void TryCatchFinally(Action action,
                                    Action<Exception> exceptionHandler,
                                    Action? finallyAction = null)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex) when(CanBeCatched(ex))
            {
                exceptionHandler.Invoke(ex);
            }
            finally
            {
                finallyAction?.Invoke();
            }
        }

        /// <inheritdoc />
        public TResult? TryCatchFinally<TResult>(Func<TResult?> func,
                                                 Action<Exception> exceptionHandler,
                                                 Action? finallyAction = null)
            where TResult : class
        {
            TResult? result = default;
            
            try
            {
                result = func.Invoke();
            }
            catch (Exception ex) when(CanBeCatched(ex))
            {
                exceptionHandler.Invoke(ex);
            }
            finally
            {
                finallyAction?.Invoke();
            }

            return result;
        }

        /// <inheritdoc />
        public TResult? TryCatchFinally<TResult>(Func<TResult?> func,
                                                 Action<Exception> exceptionHandler,
                                                 Action? finallyAction = null)
            where TResult : struct
        {
            TResult? result = default;
            
            try
            {
                result = func.Invoke();
            }
            catch (Exception ex) when(CanBeCatched(ex))
            {
                exceptionHandler.Invoke(ex);
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