using Microsoft.AspNetCore.Mvc;

using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

using Whizible26.API.Security;

using Whizible26.Application.General;

using Whizible26.Domain.Entity;



namespace Whizible26API.Controllers

{

    // Controller Added by Ajit L on 08/10/2025 for Token Generation

    // api/Auth/token and api/auth/token (case-insensitive) — legacy OAuth used /token on the old host

    [Route("api/[controller]")]

    //[Route("api/auth")]

    [ApiController]

    public class AuthController : ControllerBase

    {

        private readonly IConfiguration _configuration;

        private readonly JwtSettingsEntity _jwtSettings;

        private readonly Auth _authService;
        private readonly ITokenSessionRegistry _tokenSessionRegistry;



        public AuthController(IConfiguration configuration, ITokenSessionRegistry tokenSessionRegistry)

        {

            _configuration = configuration;

            _jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettingsEntity>() ?? new JwtSettingsEntity();

            _authService = new Auth(configuration);
            _tokenSessionRegistry = tokenSessionRegistry;

        }

        //Added and modified by Vishal Mane on 02/06/2026 to fix security issues

        [HttpPost("token")]

        public async Task<IActionResult> GetToken([FromForm] string username, [FromForm] string password, [FromForm] string grant_type)

        {

            try

            {

                var strDomain = _configuration["DomainName"];

                if (!string.IsNullOrEmpty(strDomain))

                {

                    Response.Headers.Append("Access-Control-Allow-Origin", strDomain);

                }



                if (grant_type != "password" && grant_type != "AzureAD")

                {

                    return BadRequest(new { error = "unsupported_grant_type" });

                }



                // VAPT #1 — SQL injection: block dangerous patterns in credentials (Provider.cs / Dipali V 29-Dec-2025)

                if (SqlInjectionValidator.ContainsSqlInjection(username, _configuration)

                    || SqlInjectionValidator.ContainsSqlInjection(password, _configuration))

                {

                    return BadRequest(new { error = "invalid_request", error_description = "Your request is invalid" });

                }



                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))

                {

                    return Unauthorized(new

                    {

                        error = "invalid_grant",

                        error_description = "Provided username and password is incorrect"

                    });

                }



                var loginResult = await _authService.ValidateUserAsync(username, password).ConfigureAwait(false);



                // Added by Vishal Mane on 28/05/2026 for W26API token generation for Azure/SAML case

                if (grant_type == "AzureAD")

                {

                    loginResult.IsValid = true;

                    loginResult.NormalizedUserName = TokenSessionValidator.NormalizeUserName(username);

                }

                // End of Added by Vishal Mane on 28/05/2026



                if (!loginResult.IsValid)

                {

                    return Unauthorized(new

                    {

                        error = "invalid_grant",

                        error_description = "Provided username and password is incorrect"

                    });

                }



                var tokenResult = GenerateJwtToken(loginResult);
                _tokenSessionRegistry.RegisterToken(
                    loginResult.LoginId,
                    tokenResult.ClientBinding,
                    tokenResult.TokenId,
                    tokenResult.ExpiresIn);

                return Ok(new

                {

                    access_token = tokenResult.Token,

                    token_type = "Bearer",

                    expires_in = _jwtSettings.ExpirationHours > 0 ? _jwtSettings.ExpirationHours * 3600 : 86400

                });

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine("GetToken Error: " + ex);

                return BadRequest(new { error = "invalid_request", error_description = "Something went wrong" });

            }

        }



        /// <summary>

        /// Issues JWT with session-binding and security-stamp claims (Provider.cs / Vishal Mane 27-Apr-2026).

        /// </summary>

        //Added and modified by Vishal Mane on 02/06/2026 to fix security issues
        private TokenIssueResult GenerateJwtToken(LoginValidationResult login)

        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var normalizedUser = login.NormalizedUserName ?? TokenSessionValidator.NormalizeUserName(login.DisplayUserName);

            var clientBinding = TokenSessionValidator.BuildClientBinding(HttpContext, normalizedUser);
            var tokenId = Guid.NewGuid().ToString("N");
            var expiresIn = TimeSpan.FromHours(_jwtSettings.ExpirationHours > 0 ? _jwtSettings.ExpirationHours : 24);



            var claims = new List<Claim>

            {

                new(ClaimTypes.NameIdentifier, normalizedUser),

                new(ClaimTypes.Name, normalizedUser),

                new(SecurityClaimTypes.LoginName, normalizedUser),

                new(SecurityClaimTypes.ClientBinding, clientBinding),

                new("iss", _jwtSettings.Issuer),

                new("aud", _jwtSettings.Audience),

                new("Age", _jwtSettings.Claims.Age),

                new("role", _jwtSettings.Claims.Role),

                new("userdisplayname", login.DisplayUserName ?? normalizedUser),
                new(JwtRegisteredClaimNames.Jti, tokenId)

            };



            if (!string.IsNullOrEmpty(login.LoginId))

            {

                claims.Add(new Claim(SecurityClaimTypes.LoginId, login.LoginId));

            }



            if (!string.IsNullOrEmpty(login.SecurityStamp))

            {

                claims.Add(new Claim(SecurityClaimTypes.SecurityStamp, login.SecurityStamp));

            }



            var token = new JwtSecurityToken(

                issuer: _jwtSettings.Issuer,

                audience: _jwtSettings.Audience,

                claims: claims,

                expires: DateTime.UtcNow.Add(expiresIn),

                signingCredentials: credentials);



            return new TokenIssueResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ClientBinding = clientBinding,
                TokenId = tokenId,
                ExpiresIn = expiresIn
            };

        }

        private sealed class TokenIssueResult
        {
            public string Token { get; set; } = string.Empty;
            public string ClientBinding { get; set; } = string.Empty;
            public string TokenId { get; set; } = string.Empty;
            public TimeSpan ExpiresIn { get; set; }
        }
        //End of Added and modified by Vishal Mane on 02/06/2026 to fix security issues

    }

}


