namespace SpaceEngineers.Core.Web.Api.Containers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ViewEntity
    /// </summary>
    public class ViewEntity : Dictionary<string, IDataContainer>
    {
        /// <summary> .cctor </summary>
        /// <param name="dictionary">Dictionary</param>
        public ViewEntity(IDictionary<string, IDataContainer> dictionary)
            : base(dictionary, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}