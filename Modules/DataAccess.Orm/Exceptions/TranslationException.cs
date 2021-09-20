namespace SpaceEngineers.Core.DataAccess.Orm.Exceptions
{
    using System;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// TranslationException
    /// </summary>
    public sealed class TranslationException : Exception
    {
        private const string UnableToTranslateFormat = "Unable to translate query: {0}";

        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        public TranslationException(string message)
            : base(UnableToTranslateFormat.Format(message))
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="expression">Query expression</param>
        public TranslationException(Expression expression)
            : base(UnableToTranslateFormat.Format(expression))
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="expression">Query expression</param>
        /// <param name="exception">Exception</param>
        public TranslationException(Expression expression, Exception exception)
            : base(UnableToTranslateFormat.Format(expression), exception)
        {
        }
    }
}