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
            container.Register<QueryExpressionsCollector, QueryExpressionsCollector>(EnLifestyle.Singleton);
            container.RegisterDecorator<IQueryProvider, QueryProviderDecorator>(EnLifestyle.Singleton);
            container.RegisterDecorator<IAsyncQueryProvider, AsyncQueryProviderDecorator>(EnLifestyle.Singleton);
        }
    }
}