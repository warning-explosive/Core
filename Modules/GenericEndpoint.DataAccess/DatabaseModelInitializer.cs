namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.Unregistered)]
    internal class DatabaseModelInitializer : IEndpointInitializer
    {
        private readonly IDomainTypeProvider _domainTypeProvider;
        private readonly IDataBaseModelBuilder _dataBaseModelBuilder;

        public DatabaseModelInitializer(
            IDomainTypeProvider domainTypeProvider,
            IDataBaseModelBuilder dataBaseModelBuilder)
        {
            _domainTypeProvider = domainTypeProvider;
            _dataBaseModelBuilder = dataBaseModelBuilder;
        }

        public Task Initialize(CancellationToken token)
        {
            /*
             * TODO: 1. Build model tree from code
             * TODO: 2. Build model tree from database (if exists)
             * TODO: 3. Compare, extract diff, generate migration
             * TODO: 4. Apply migration (initial migration or regular migration)
             */

            var model = new Dictionary<Type, ModelNode>();

            foreach (var entity in _domainTypeProvider.Entities())
            {
                if (model.ContainsKey(entity))
                {
                    continue;
                }

                foreach (var node in _dataBaseModelBuilder.BuildNodes(entity))
                {
                    if (!model.ContainsKey(node.NodeType))
                    {
                        model[entity] = node;
                    }
                }
            }

            throw new NotImplementedException();
        }
    }
}