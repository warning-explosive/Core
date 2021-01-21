namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;

    /// <summary>
    /// Synchronization primitives extensions
    /// </summary>
    public static class SynchronizationPrimitivesExtensions
    {
        /// <summary>
        /// Opens read-lock scope
        /// </summary>
        /// <param name="sync">ReaderWriterLockSlim</param>
        /// <returns>Opened scope and its cancellation</returns>
        public static IDisposable WithinReadLock(this ReaderWriterLockSlim sync)
        {
            if (sync.IsReadLockHeld)
            {
                return Disposable.Empty;
            }

            sync.EnterReadLock();

            return Disposable.Create(sync.ExitReadLock);
        }

        /// <summary>
        /// Invoke action within read-lock
        /// </summary>
        /// <param name="sync">ReaderWriterLockSlim</param>
        /// <param name="action">Action</param>
        public static void WithinReadLock(this ReaderWriterLockSlim sync, Action action)
        {
            if (sync.IsReadLockHeld)
            {
                action.Invoke();
                return;
            }

            sync.EnterReadLock();

            try
            {
                action.Invoke();
            }
            finally
            {
                sync.ExitReadLock();
            }
        }

        /// <summary>
        /// Invoke function within read-lock
        /// </summary>
        /// <param name="sync">ReaderWriterLockSlim</param>
        /// <param name="func">Function</param>
        /// <typeparam name="T">Return value type-argument</typeparam>
        /// <returns>Function result</returns>
        public static T WithinReadLock<T>(this ReaderWriterLockSlim sync, Func<T> func)
        {
            if (sync.IsReadLockHeld)
            {
                return func.Invoke();
            }

            sync.EnterReadLock();

            try
            {
                return func.Invoke();
            }
            finally
            {
                sync.ExitReadLock();
            }
        }

        /// <summary>
        /// Opens write-lock scope
        /// </summary>
        /// <param name="sync">ReaderWriterLockSlim</param>
        /// <returns>Opened scope and its cancellation</returns>
        public static IDisposable WithinWriteLock(this ReaderWriterLockSlim sync)
        {
            sync.EnterWriteLock();

            return Disposable.Create(sync.ExitWriteLock);
        }

        /// <summary>
        /// Invoke action within write-lock
        /// </summary>
        /// <param name="sync">ReaderWriterLockSlim</param>
        /// <param name="action">Action</param>
        public static void WithinWriteLock(this ReaderWriterLockSlim sync, Action action)
        {
            sync.EnterWriteLock();

            try
            {
                action.Invoke();
            }
            finally
            {
                sync.ExitWriteLock();
            }
        }

        /// <summary>
        /// Invoke function within write-lock
        /// </summary>
        /// <param name="sync">ReaderWriterLockSlim</param>
        /// <param name="func">Function</param>
        /// <typeparam name="T">Return value type-argument</typeparam>
        /// <returns>Function result</returns>
        public static T WithinWriteLock<T>(this ReaderWriterLockSlim sync, Func<T> func)
        {
            sync.EnterWriteLock();

            try
            {
                return func.Invoke();
            }
            finally
            {
                sync.ExitWriteLock();
            }
        }
    }
}