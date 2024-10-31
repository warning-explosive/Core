namespace SpaceEngineers.Core.Basics;

using System;
using System.Threading;
using Disposables;

public static class SynchronizationPrimitivesExtensions
{
    public static IDisposable WithinReadLock(this ReaderWriterLockSlim sync)
    {
        if (sync.IsReadLockHeld)
        {
            return Disposable.Empty;
        }

        sync.EnterReadLock();

        return Disposable.Create(sync.ExitReadLock);
    }

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

    public static IDisposable WithinWriteLock(this ReaderWriterLockSlim sync)
    {
        sync.EnterWriteLock();

        return Disposable.Create(sync.ExitWriteLock);
    }

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