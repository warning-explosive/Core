namespace SpaceEngineers.Core.DataAccess.Orm.Exceptions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// TranslationException
    /// </summary>
    public sealed class TranslationException : Exception
    {
        private const string UnableToTranslateFormat = "Unable to translate query: {0}";

        /// <summary> .cctor </summary>
        /// <param name="expression">Query expression</param>
        /// <param name="exception">Exception</param>
        public TranslationException(Expression expression, Exception exception)
            : base(string.Format(UnableToTranslateFormat, expression), exception)
        {
        }
    }
}