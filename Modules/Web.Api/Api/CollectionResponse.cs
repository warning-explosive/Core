namespace SpaceEngineers.Core.Web.Api.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collection response
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public class CollectionResponse<T> : BaseResponse
    {
        /// <summary>
        /// .cctor
        /// </summary>
        public CollectionResponse()
        {
            Items = Array.Empty<T>();
        }

        /// <summary>
        /// Items
        /// </summary>
        public IReadOnlyCollection<T> Items { get; private set; }

        /// <summary>
        /// With items
        /// </summary>
        /// <param name="items">Items</param>
        /// <returns>CollectionResponse</returns>
        public CollectionResponse<T> WithItems(IEnumerable<T> items)
        {
            Items = items.ToList();
            return this;
        }

        /// <summary>
        /// With error
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>CollectionResponse</returns>
        public CollectionResponse<T> WithError(Exception exception)
        {
            AddError(exception);
            return this;
        }

        /// <summary>
        /// With error
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>CollectionResponse</returns>
        public CollectionResponse<T> WithError(string message)
        {
            AddError(message);
            return this;
        }
    }
}