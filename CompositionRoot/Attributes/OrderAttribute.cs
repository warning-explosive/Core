namespace SpaceEngineers.Core.CompositionRoot.Attributes
{
    using System;

    /// <summary>
    /// Attribute which defines order
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OrderAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="order">Order</param>
        public OrderAttribute(uint order)
        {
            Order = order;
        }

        /// <summary>
        /// Order
        /// </summary>
        public uint Order { get; }
    }
}