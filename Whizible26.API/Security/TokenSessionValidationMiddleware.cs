using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Whizible26.API.Security
{
    public class TokenSessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ITokenSessionRegistry _tokenSessionRegistry;

        public TokenSessionValidationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ITokenSessionRegistry tokenSessionRegistry)
        {
            _next = next;
            _configuration = configuration;
            _tokenSessionRegistry = tokenSessionRegistry;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!RequiresAuthorization(context))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            if (!TryGetBearerToken(context, out var rawToken))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            if (!IsPersonalJwt(rawToken))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated != true || context.User.Identity is not ClaimsIdentity identity)
            {
                await WriteUnauthorizedAsync(context, "invalid_token").ConfigureAwait(false);
                return;
            }

            if (!TokenSessionValidator.IsTokenBoundToCurrentRequest(identity, context, _configuration))
            {
                await WriteUnauthorizedAsync(context, "invalid_token").ConfigureAwait(false);
                return;
            }

            if (!await TokenSessionValidator.IsSecurityStampValidAsync(identity, context, _configuration, context.RequestAborted).ConfigureAwait(false))
            {
                await WriteUnauthorizedAsync(context, "PASSWORDCHANGED").ConfigureAwait(false);
                return;
            }

            if (_configuration.GetValue("Security:RequireRegisteredToken", true) && !IsRegisteredToken(identity))
            {
                await WriteUnauthorizedAsync(context, "invalid_token").ConfigureAwait(false);
                return;
            }

            await _next(context).ConfigureAwait(false);
        }

        private bool IsRegisteredToken(ClaimsIdentity identity)
        {
            var loginId = identity.FindFirst(SecurityClaimTypes.LoginId)?.Value;
            var clientBinding = identity.FindFirst(SecurityClaimTypes.ClientBinding)?.Value;
            var jti = identity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrWhiteSpace(jti))
            {
                return !_configuration.GetValue("Security:RequireTokenJti", true);
            }

            if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(clientBinding))
            {
                return false;
            }

            return _tokenSessionRegistry.IsTokenActive(loginId, clientBinding, jti);
        }

        private static bool RequiresAuthorization(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                return false;
            }

            if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                return false;
            }

            return endpoint.Metadata.GetMetadata<IAuthorizeData>() != null;
        }

        private static bool TryGetBearerToken(HttpContext context, out string token)
        {
            token = string.Empty;
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            token = authHeader["Bearer ".Length..].Trim();
            return !string.IsNullOrWhiteSpace(token);
        }

        private bool IsPersonalJwt(string rawToken)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(rawToken);
                var issuer = jwt.Issuer ?? string.Empty;
                if (issuer.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase)
                    || issuer.Contains("sts.windows.net", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var expectedIssuer = _configuration["JwtSettings:Issuer"] ?? string.Empty;
                return string.IsNullOrEmpty(expectedIssuer)
                    || string.Equals(jwt.Issuer, expectedIssuer, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context, string error)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer error=\"" + error + "\"";
            await context.Response.WriteAsJsonAsync(new { error }).ConfigureAwait(false);
        }
    }
}
