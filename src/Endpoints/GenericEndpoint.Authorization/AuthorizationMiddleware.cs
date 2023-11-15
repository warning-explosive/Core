namespace SpaceEngineers.Core.GenericEndpoint.Authorization
{
    using System;
    using System.Linq;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Contract.Attributes;
    using JwtAuthentication;
    using Pipeline;

    /// <summary>
    /// AuthorizationMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    [After(typeof(ErrorHandlingMiddleware))]
    [Before(typeof(UnitOfWorkMiddleware))]
    public class AuthorizationMiddleware : IMessageHandlerMiddleware,
                                           ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly ITokenProvider _tokenProvider;

        /// <summary> .cctor </summary>
        /// <param name="tokenProvider">ITokenProvider</param>
        public AuthorizationMiddleware(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        /// <inheritdoc />
        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            if (!context.Message.ReflectedType.HasAttribute<AllowAnonymousAttribute>())
            {
                var authorizationToken = context
                    .Message
                    .ReadHeader<Authorization>()
                   ?.Value ?? throw new SecurityException("Unauthorized");

                var grantedPermissions = _tokenProvider.GetPermissions(authorizationToken);

                var requiredFeatures = context
                    .Message
                    .ReflectedType
                    .GetRequiredAttribute<FeatureAttribute>()
                    .Features;

                var accessGranted = requiredFeatures.All(requiredFeature => grantedPermissions.Contains(requiredFeature));

                if (!accessGranted)
                {
                    throw new SecurityException("Forbidden");
                }
            }

            await next(context, token).ConfigureAwait(false);
        }
    }
}