namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;

    /// <summary>
    /// ICompositeDisposable
    /// disposes like stack - LIFO
    /// </summary>
    public interface ICompositeDisposable : IDisposable
    {
        /// <summary> Push </summary>
        /// <param name="disposable">IDisposable</param>
        void Push(IDisposable disposable);
    }
}