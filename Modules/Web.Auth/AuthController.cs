namespace SpaceEngineers.Core.Web.Auth
{
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Api.Api;
    using Basics;
    using GenericEndpoint.Contract.Attributes;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Auth controller
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Authenticates user by default authentication handler (basic authentication)
        /// </summary>
        /// <returns>Authentication result</returns>
        [Feature(AuthEndpoint.Contract.Features.Authentication)]
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