namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using Mocks;

    internal class QueryExpressionsCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<QueryExpressionsCollector, QueryExpressionsCollector>(EnLifestyle.Scoped);
        }
    }
}