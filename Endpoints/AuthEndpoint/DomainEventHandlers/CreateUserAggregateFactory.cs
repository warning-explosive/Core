namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Domain;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class CreateUserAggregateFactory : IAggregateFactory<User, CreateUserSpecification>,
                                                IResolvable<IAggregateFactory<User, CreateUserSpecification>>
    {
        public Task<User> Build(CreateUserSpecification spec, CancellationToken token)
        {
            return Task.FromResult(new User(new Username(spec.Username), new Password(spec.Password)));
        }
    }
}