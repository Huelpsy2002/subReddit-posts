using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApplication1.Data;

namespace WebApplication1.busnessLogic
{
    public class HandleLogic
    {
        private ClientSettings _Clientsettings;
        private IConfiguration _configuration;
        private readonly DataBase _database;
        public HandleLogic(IOptions<ClientSettings> options, IConfiguration configuration, DataBase dataBase)
        {
            _Clientsettings = options.Value;
            _configuration = configuration;
            _database = dataBase;
        }



        private string GenerateJwt(DateTime expire_at, string redditToken, string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("jwtSecretKey"));

            var claims = new[]
               {
                    new Claim("reddit_token", redditToken),
                    new Claim("reddit_refresh", refreshToken ?? ""),
                    new Claim("reddit_exp", ((DateTimeOffset)(expire_at)).ToUnixTimeSeconds().ToString())

                };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Expires = expire_at,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        public async Task<(string, DateTime)> ExchangeCodeForAccessTokenAsync(string code)
        {


            using var httpClient = new HttpClient();
        

            // Create the Basic Auth header
            var authBytes = Encoding.UTF8.GetBytes($"{_Clientsettings.ClientId}:{_Clientsettings.ClientSecret}");
            var authHeader = Convert.ToBase64String(authBytes);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            httpClient.DefaultRequestHeaders.Add("User-Agent", _Clientsettings.AppName);

            // Form data to send
            var formData = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", _Clientsettings.RedirectUri }
                };

            var content = new FormUrlEncodedContent(formData);

            var response = await httpClient.PostAsync("https://www.reddit.com/api/v1/access_token", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Token exchange failed: {errorContent}");
            }
            await _database.access(); //connecting to database after getting the access token
            var json = await response.Content.ReadAsStringAsync();
            var redditTokenResponse = JsonSerializer.Deserialize<RedditTokenResponse>(json);
            var expires_at = DateTime.UtcNow.AddSeconds(redditTokenResponse.expires_in);

            var jwt = GenerateJwt(expires_at, redditTokenResponse.refresh_token, redditTokenResponse.refresh_token);
            TokenStore.Tokens[jwt] = new RedditTokenInfo
            {
                AccessToken = redditTokenResponse.access_token,
                RefreshToken = redditTokenResponse.refresh_token,
                ExpiresAt = expires_at
            };
            _database.insertTokken(jwt, redditTokenResponse.access_token, redditTokenResponse.refresh_token, expires_at);


            return (jwt, expires_at);
        }
        public async Task<Token> getToken(string jwt)
        {
        
            try
            {

                

                var token = await _database.getToken(jwt);
                if (token != null)
                {
                    Console.WriteLine("token :" + token);
                }
                else
                {
                    Console.WriteLine("null");
                }
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token fetch failed:");
                Console.WriteLine(ex.Message);
                return null;

            }

        } 
    }
}
