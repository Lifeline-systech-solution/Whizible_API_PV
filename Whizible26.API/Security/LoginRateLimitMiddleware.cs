using System.Collections.Concurrent;

namespace Whizible26.API.Security
{
    /// <summary>
    /// Brute-force rate limit for login token endpoint only (VAPT).
    /// Legacy OWIN middleware counted all paths; that breaks parallel LandingDB calls and Burp replay (429 + false CORS errors).
    /// </summary>
    public class LoginRateLimitMiddleware
    {
        private static readonly ConcurrentDictionary<string, (int Count, DateTime Timestamp)> RequestCounts = new();
        private static readonly object SyncRoot = new();

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public LoginRateLimitMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var isLoginTokenEndpoint = IsLoginTokenEndpoint(path, context.Request.Method);

            // POST-only guard applies only to login token URL (legacy Startup.cs)
            if (isLoginTokenEndpoint && !HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await context.Response.WriteAsync("Only POST method is allowed").ConfigureAwait(false);
                return;
            }

            // Rate limit only POST /api/auth/token — not LandingDB or other APIs
            if (!isLoginTokenEndpoint
                || !_configuration.GetValue("Security:RateLimitEnabled", true))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            var durationMinutes = _configuration.GetValue("Security:RateLimit_Duration_Minutes", 5);
            var maxCount = _configuration.GetValue("Security:RateLimit_Count", 3);

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userKey = string.Empty;

            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync().ConfigureAwait(false);
                userKey = form["username"].ToString();
            }

            var key = $"{ip}_{path}_{userKey}";
            var rateLimited = false;

            lock (SyncRoot)
            {
                if (!RequestCounts.ContainsKey(key))
                {
                    RequestCounts[key] = (1, DateTime.UtcNow);
                }
                else
                {
                    var entry = RequestCounts[key];
                    if ((DateTime.UtcNow - entry.Timestamp).TotalMinutes < durationMinutes)
                    {
                        if (entry.Count >= maxCount)
                        {
                            rateLimited = true;
                        }
                        else
                        {
                            RequestCounts[key] = (entry.Count + 1, entry.Timestamp);
                        }
                    }
                    else
                    {
                        RequestCounts[key] = (1, DateTime.UtcNow);
                    }
                }
            }

            if (rateLimited)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many attempts for this request").ConfigureAwait(false);
                return;
            }

            await _next(context).ConfigureAwait(false);
        }

        /// <summary>
        /// True for POST api/Auth/token or api/auth/token (OAuth-style login).
        /// </summary>
        internal static bool IsLoginTokenEndpoint(string path, string method)
        {
            if (!HttpMethods.IsPost(method))
            {
                return false;
            }

            return path.Contains("/token", StringComparison.OrdinalIgnoreCase)
                && path.Contains("/auth", StringComparison.OrdinalIgnoreCase);
        }
    }
}
