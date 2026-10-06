using Microsoft.Extensions.Caching.Memory;

namespace Whizible26.API.Security
{
    public class TokenSessionRegistry : ITokenSessionRegistry
    {
        private readonly IMemoryCache _cache;

        public TokenSessionRegistry(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void RegisterToken(string loginId, string clientBinding, string jti, TimeSpan lifetime)
        {
            if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(clientBinding) || string.IsNullOrWhiteSpace(jti))
            {
                return;
            }

            _cache.Set(BuildKey(loginId, clientBinding), jti, lifetime);
        }

        public bool IsTokenActive(string loginId, string clientBinding, string jti)
        {
            if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(clientBinding) || string.IsNullOrWhiteSpace(jti))
            {
                return false;
            }

            return _cache.TryGetValue(BuildKey(loginId, clientBinding), out string? registered)
                && string.Equals(registered, jti, StringComparison.Ordinal);
        }

        public void RevokeLogin(string loginId)
        {
            if (string.IsNullOrWhiteSpace(loginId))
            {
                return;
            }

            // no-op for in-memory implementation
        }

        private static string BuildKey(string loginId, string clientBinding)
            => "auth:" + loginId.Trim() + ":" + clientBinding;
    }
}
