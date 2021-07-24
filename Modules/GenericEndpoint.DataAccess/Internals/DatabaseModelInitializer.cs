namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Internals
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Core.DataAccess.Orm.Model.Abstractions;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.Unregistered)]
    internal class DatabaseModelInitializer : IEndpointInitializer,
                                              ICollectionResolvable<IEndpointInitializer>
    {
        private readonly IDatabaseModelBuilder _databaseModelBuilder;
        private readonly ICodeModelBuilder _codeModelBuilder;
        private readonly IDatabaseModelComparator _databaseModelComparator;

        public DatabaseModelInitializer(
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IDatabaseModelComparator databaseModelComparator)
        {
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _databaseModelComparator = databaseModelComparator;
        }

        public Task Initialize(CancellationToken token)
        {
            var actualModel = _databaseModelBuilder.BuildModel();
            var expectedModel = _codeModelBuilder.BuildModel();

            // TODO: Compare, extract diff, generate migration
            var modelChanges = _databaseModelComparator
                .ExtractDiff(actualModel, expectedModel)
                .ToList();

            // TODO: Apply migration (initial migration or regular migration)
            return Task.CompletedTask;
        }
    }
}