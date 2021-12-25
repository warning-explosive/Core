namespace SpaceEngineers.Core.Web.Api.Api
{
    using System;

    /// <summary>
    /// Scalar response
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public class ScalarResponse<T> : BaseResponse
    {
        /// <summary>
        /// Item
        /// </summary>
        public T? Item { get; private set; }

        /// <summary>
        /// With item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>ScalarResponse</returns>
        public ScalarResponse<T> WithItem(T item)
        {
            Item = item;
            return this;
        }

        /// <summary>
        /// With error
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>ScalarResponse</returns>
        public ScalarResponse<T> WithError(Exception exception)
        {
            AddError(exception);
            return this;
        }

        /// <summary>
        /// With error
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>ScalarResponse</returns>
        public ScalarResponse<T> WithError(string message)
        {
            AddError(message);
            return this;
        }
    }
}