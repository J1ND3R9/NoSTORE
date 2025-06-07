namespace NoSTORE.Settings
{
    public class AuthSettings
    {
        public int TokenLifetimeMinutes { get; set; }
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
