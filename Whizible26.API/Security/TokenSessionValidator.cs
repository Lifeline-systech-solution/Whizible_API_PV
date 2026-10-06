using CommonFunctions;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Whizible26.API.Security
{
    public static class TokenSessionValidator
    {
        public static bool IsTokenBoundToCurrentRequest(ClaimsIdentity? identity, HttpContext httpContext, IConfiguration configuration)
        {
            if (!configuration.GetValue("Security:SessionBindingEnabled", true))
            {
                return true;
            }

            if (identity == null || httpContext == null)
            {
                return false;
            }

            var tokenUser = NormalizeUserName(identity.FindFirst(SecurityClaimTypes.LoginName)?.Value ?? identity.Name);
            var tokenBinding = identity.FindFirst(SecurityClaimTypes.ClientBinding)?.Value;
            if (string.IsNullOrWhiteSpace(tokenUser) || string.IsNullOrWhiteSpace(tokenBinding))
            {
                return false;
            }

            var expectedBinding = BuildClientBinding(httpContext, tokenUser);
            return SecureEquals(tokenBinding, expectedBinding);
        }

        public static async Task<bool> IsSecurityStampValidAsync(
            ClaimsIdentity? identity,
            HttpContext httpContext,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            if (!configuration.GetValue("Security:SecurityStampValidationEnabled", true))
            {
                return true;
            }

            try
            {
                if (identity == null)
                {
                    return true;
                }

                var loginId = identity.FindFirst(SecurityClaimTypes.LoginId)?.Value;
                var tokenStamp = identity.FindFirst(SecurityClaimTypes.SecurityStamp)?.Value;

                if (string.IsNullOrEmpty(loginId))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(tokenStamp))
                {
                    return false;
                }

                var lastCheckKey = "_StampLastCheck_" + loginId;
                if (httpContext.Items.TryGetValue(lastCheckKey, out var lastCheckObj)
                    && lastCheckObj is DateTime lastCheck
                    && (DateTime.UtcNow - lastCheck).TotalSeconds < 60)
                {
                    return true;
                }

                httpContext.Items[lastCheckKey] = DateTime.UtcNow;

                var connectionString = BuildConnectionString(configuration);
                if (string.IsNullOrEmpty(connectionString))
                {
                    return true;
                }

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using var command = new SqlCommand("EXEC usp_Sel_SecurityStamp_For_LoginID @LoginID", connection);
                command.Parameters.AddWithValue("@LoginID", loginId);

                var dbStampObj = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                var dbStamp = CommonFunctions.General.CheckIsNothing(
                    CommonFunctions.Data.CheckIsDBNull(dbStampObj, ""), "")?.ToString() ?? "";

                if (string.IsNullOrEmpty(dbStamp))
                {
                    return false;
                }

                return string.Equals(dbStamp, tokenStamp, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string NormalizeUserName(string? userName)
            => (userName ?? string.Empty).Trim().ToLowerInvariant();

        public static string BuildClientBinding(HttpContext httpContext, string normalizedUser)
        {
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var raw = normalizedUser + "|" + userAgent + "|" + ipAddress;
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private static bool SecureEquals(string? left, string? right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            var rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);
            if (leftBytes.Length != rightBytes.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < leftBytes.Length; i++)
            {
                diff |= leftBytes[i] ^ rightBytes[i];
            }

            return diff == 0;
        }

        private static string? BuildConnectionString(IConfiguration configuration)
        {
            var encrypted = configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value;
            if (string.IsNullOrEmpty(encrypted))
            {
                return null;
            }

            return General.BuildConnectionString(encrypted);
        }
    }
}
