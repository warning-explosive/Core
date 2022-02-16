namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System.Linq;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using DataAccess.Orm.Linq;
    using Mocks;

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