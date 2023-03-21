namespace SpaceEngineers.Core.Web.Auth
{
    using System.Collections.Generic;

    internal interface IWebApiFeaturesProvider
    {
        public IReadOnlyCollection<string> GetFeatures(string controller, string action, string verb);
    }
}