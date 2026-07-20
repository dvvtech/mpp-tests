namespace MppTests.Api.Configuration
{
    public sealed class ProxyConfig
    {
        public const string SectionName = "ProxyConfig";

        public string Url { get; init; }
        public string Login { get; init; }
        public string Password { get; init; }
    }
}
