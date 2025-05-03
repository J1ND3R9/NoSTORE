namespace NoSTORE.Settings
{
    public class AuthSettings
    {
        public TimeSpan Expires { get; set; }
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
