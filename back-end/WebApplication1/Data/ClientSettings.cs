namespace WebApplication1.Data
{
    public class ClientSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string AppName { get; set; }
    }

    public class CodeRequest
    {
        public string Code { get; set; }
    }

    public class RedditTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
    }




    public class RedditTokenInfo
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
    public class TokenStore
    {
        public static Dictionary<string, RedditTokenInfo> Tokens = new();
    }

}
