namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using CompositionRoot.Registration;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class QueryExpressionsCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<QueryExpressionsCollector, QueryExpressionsCollector>(EnLifestyle.Scoped);
        }
    }
}