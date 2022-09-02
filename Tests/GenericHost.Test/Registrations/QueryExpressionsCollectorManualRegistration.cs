namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using System.Linq;
    using CompositionRoot.Registration;
    using DataAccess.Orm.Linq;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class QueryExpressionsCollectorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<QueryExpressionsCollector, QueryExpressionsCollector>(EnLifestyle.Scoped);
            container.RegisterDecorator<IQueryProvider, QueryProviderDecorator>(EnLifestyle.Scoped);
            container.RegisterDecorator<IAsyncQueryProvider, AsyncQueryProviderDecorator>(EnLifestyle.Scoped);
        }
    }
}