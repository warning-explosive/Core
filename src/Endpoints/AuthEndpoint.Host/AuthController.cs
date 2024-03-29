namespace SpaceEngineers.Core.AuthEndpoint.Host
{
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Basics;
    using Contract;
    using GenericEndpoint.Authorization.Web;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using SpaceEngineers.Core.GenericEndpoint.Contract.Attributes;
    using Web.Api;

    /// <summary>
    /// Auth controller
    /// </summary>
    [Route("api/[controller]/[action]")]
    [EndpointGroupName(Identity.LogicalName)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Authenticates user by default authentication handler (basic authentication)
        /// </summary>
        /// <returns>Authentication result</returns>
        [Feature(Features.Authentication)]
        [HttpGet]
        public Task<ActionResult<ScalarResponse<string>>> AuthenticateUser()
        {
            var authorizationToken = HttpContext.GetAuthorizationToken();

            var response = new ScalarResponse<string>();

            _ = authorizationToken.IsNullOrWhiteSpace()
                ? response.WithError("Unauthorized")
                : response.WithItem(authorizationToken);

            var result = new JsonResult(response)
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = (int)(response.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError)
            };

            return Task.FromResult<ActionResult<ScalarResponse<string>>>(result);
        }
    }
}