namespace Whizible26.API.Security
{
    /// <summary>
    /// JWT claim types aligned with legacy OAuth Provider (WhizibleAPI CS Files/Provider.cs).
    /// </summary>
    public static class SecurityClaimTypes
    {
        // Added by Vishal Mane on 29/05/2026 — session binding and login metadata (VAPT: improper session management)
        public const string LoginName = "login_name";
        public const string ClientBinding = "client_binding";
        public const string LoginId = "login_id";
        public const string SecurityStamp = "security_stamp";
    }
}
