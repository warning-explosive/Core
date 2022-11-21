namespace SpaceEngineers.Core.Web.Auth
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Security.Claims;
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
        [Feature("Authentication")]
        [HttpGet]
        public Task<ActionResult<ScalarResponse<string>>> AuthenticateUser()
        {
            var token = HttpContext.User.FindFirst(ClaimTypes.Authentication)?.Value;

            if (string.IsNullOrEmpty(token))
            {
                var schema = AuthenticationHeaderValue
                    .Parse(HttpContext.Request.Headers.Authorization)
                    .Scheme;

                token = HttpContext.Request.Headers.Authorization.ToString()[schema.Length..].Trim();
            }

            var response = new ScalarResponse<string>();

            if (!token.IsNullOrWhiteSpace())
            {
                response.WithItem(token);
            }
            else
            {
                response.WithError("Wrong username or password");
            }

            var result = new JsonResult(response)
            {
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = (int)(response.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError)
            };

            return Task.FromResult<ActionResult<ScalarResponse<string>>>(result);
        }
    }
}