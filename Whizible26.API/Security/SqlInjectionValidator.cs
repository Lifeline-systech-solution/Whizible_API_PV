using Microsoft.Extensions.Configuration;

namespace Whizible26.API.Security
{
    /// <summary>
    /// Blocks obvious SQL injection patterns in login and other user-supplied strings.
    /// Ported from WhizibleAPI Provider.ContainsSQLInjection (reads keywords from security.config / appsettings).
    /// </summary>
    public static class SqlInjectionValidator
    {
        /// <summary>
        /// Returns true when input contains a configured SQL keyword (case-insensitive substring match).
        /// </summary>
        public static bool ContainsSqlInjection(string? input, IConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var keywords = configuration["Security:SQLKeyWords"]
                ?? configuration["SQLKeyWords"];

            if (string.IsNullOrWhiteSpace(keywords))
            {
                return false;
            }

            var words = keywords.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var lowerInput = input.ToLowerInvariant();

            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word) && lowerInput.Contains(word.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
