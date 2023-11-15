namespace SpaceEngineers.Core.Web.Api.Containers
{
    using System;

    /// <summary>
    /// DateTimeDataContainer
    /// </summary>
    public class DateTimeDataContainer : GenericDataContainer<DateTime?>
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Value</param>
        public DateTimeDataContainer(DateTime? value)
            : base(EnContainerType.DateTime, value)
        {
        }
    }
}