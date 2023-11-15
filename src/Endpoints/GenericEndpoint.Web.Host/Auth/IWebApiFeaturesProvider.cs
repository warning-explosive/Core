namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Auth
{
    using System.Collections.Generic;

    internal interface IWebApiFeaturesProvider
    {
        public IReadOnlyCollection<string> GetFeatures(string controller, string action, string verb);
    }
}