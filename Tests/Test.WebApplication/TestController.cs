namespace SpaceEngineers.Core.Test.WebApplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Json;
    using GenericEndpoint.Contract;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using RestSharp;
    using Web.Api.Api;
    using Web.Api.Containers;

    /// <summary>
    /// Test controller
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDataContainersProvider _dataContainersProvider;

        /// <summary> .cctor </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        /// <param name="dataContainersProvider">IDataContainersProvider</param>
        public TestController(
            EndpointIdentity endpointIdentity,
            IJsonSerializer jsonSerializer,
            IDataContainersProvider dataContainersProvider)
        {
            _endpointIdentity = endpointIdentity;
            _jsonSerializer = jsonSerializer;
            _dataContainersProvider = dataContainersProvider;
        }

        /// <summary>
        /// Gets authorized username
        /// </summary>
        /// <returns>Authorized username</returns>
        [HttpGet]
        public async Task<ActionResult<ScalarResponse<string>>> GetUsername()
        {
            /*
             * Authorization steps:
             * 1. Log on
             * 2. Call IdentityProvider and receive identity token
             * 3. Call AuthorizationProvider and receive API/Service/UI-specific roles and permissions
             *    AuthorizationProvider maps identity data (who) to authorization data (permission and roles - what principal can do)
             *    Mappings can be static (common rules, conditions and conventions) or dynamic (claims transformations)
             *
             * Each application (web-api endpoint, micro-service, UI application, etc.) should have their scoped roles, permissions and mapping rules.
             * Application can register authorization client so as to ask for authorization data from authorization service.
             * For dynamically defined policies we can use custom PolicyProvider for policy-based authorization (ASP.NET CORE) and inject policy name into AuthorizeAttribute.
             */

            var response = new ScalarResponse<string>();

            var username = User.FindFirst(ClaimTypes.Name).Value;
            response.WithItem(username);

            var result = new JsonResult(response)
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = (int)(response.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError)
            };

            return await Task.FromResult(result).ConfigureAwait(false);
        }

        /// <summary>
        /// Anonymously gets application info
        /// </summary>
        /// <returns>Application info</returns>
        [HttpGet]
        [AllowAnonymous]
        [SuppressMessage("Analysis", "CA1031", Justification = "desired behavior")]
        public async Task<ActionResult<ScalarResponse<string>>> ApplicationInfo()
        {
            var response = new ScalarResponse<string>();

            try
            {
                response.WithItem($"{_endpointIdentity}");
            }
            catch (Exception exception)
            {
                response.WithError(exception);
            }

            var result = new JsonResult(response)
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = (int)(response.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError)
            };

            return await Task.FromResult(result).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads posts from https://jsonplaceholder.typicode.com/posts and converts to view entity
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Post view entities</returns>
        [HttpGet]
        [SuppressMessage("Analysis", "CA1031", Justification = "desired behavior")]
        public async Task<ActionResult<CollectionResponse<ViewEntity>>> ReadFakePosts(CancellationToken token)
        {
            var response = new CollectionResponse<ViewEntity>();

            try
            {
                RestResponse restResponse;

                using (var client = new RestClient())
                {
                    restResponse = await client
                        .ExecuteAsync(new RestRequest("https://jsonplaceholder.typicode.com/posts", Method.Get), token)
                        .ConfigureAwait(false);
                }

                if (restResponse.Content == null)
                {
                    response.WithError("Response body is empty");
                }
                else
                {
                    var items = _jsonSerializer.DeserializeObject<Post[]>(restResponse.Content);
                    var viewItems = items.Select(_dataContainersProvider.ToViewEntity).ToArray();

                    response.WithItems(viewItems);
                }
            }
            catch (Exception exception)
            {
                response.WithError(exception);
            }

            var result = new JsonResult(response)
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = (int)(response.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError)
            };

            return result;
        }

        private class Post
        {
            public string Id { get; set; } = null!;

            public string UserId { get; set; } = null!;

            public string Title { get; set; } = null!;

            public string Body { get; set; } = null!;
        }
    }
}