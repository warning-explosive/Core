namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class CreateUserAggregateFactory : IAggregateFactory<User, CreateUserSpecification>,
                                                IResolvable<IAggregateFactory<User, CreateUserSpecification>>
    {
        public Task<User> Build(CreateUserSpecification spec, CancellationToken token)
        {
            return Task.FromResult(new User(spec.Username, spec.Password));
        }
    }
}