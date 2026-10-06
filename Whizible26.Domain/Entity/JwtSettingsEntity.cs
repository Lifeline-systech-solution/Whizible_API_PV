namespace Whizible26.Domain.Entity
{
    public class JwtSettingsEntity
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationHours { get; set; } = 24;
        public JwtClaimsEntity Claims { get; set; } = new JwtClaimsEntity();
    }

    public class JwtClaimsEntity
    {
        public string Age { get; set; } = "16";
        public string Role { get; set; } = "admin";
    }
}
