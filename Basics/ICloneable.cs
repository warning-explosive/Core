namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// ICloneable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface ICloneable<out T> : ICloneable
    {
        /// <summary> Clone </summary>
        /// <returns>Copy</returns>
        new T Clone();
    }
}