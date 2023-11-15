namespace SpaceEngineers.Core.Basics.Exceptions
{
    using System;

    /// <summary>
    /// NotFoundException
    /// </summary>
    public sealed class NotFoundException : Exception
    {
        /// <summary> .ctor </summary>
        /// <param name="message">Exception message</param>
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}