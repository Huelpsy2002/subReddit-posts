using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text;
using System.Text.Json;
using WebApplication1.busnessLogic;
using WebApplication1.Data;


namespace WebApplication1.Controllers


{
  



    [Route("api")]
    [ApiController]
    public class RedditController : ControllerBase
    {
       private readonly HandleLogic _handleLogic ;
       public RedditController(HandleLogic handleLogic)
        {
            _handleLogic = handleLogic;
        }


        [HttpPost("callBack")]
        public async Task <IActionResult> receiveCodeAndGenerateJwt([FromBody] CodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { message = "Code is required" });
            }
            try
            {
                var (jwt,expires_at) = await _handleLogic.ExchangeCodeForAccessTokenAsync(request.Code);
                Response.Cookies.Append("token", jwt, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, 
                    SameSite = SameSiteMode.Strict,
                    Expires = expires_at,
                });
                
                return Ok(new { message = "Login successful" });
                

            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return BadRequest(new { message = error.Message });
            }
        }



        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetRedditMe()
        {
            var jwt = Request.Cookies["token"]; ;
            var tokenInfo = await _handleLogic.getToken(jwt);
            // if (!TokenStore.Tokens.TryGetValue(jwt, out var tokenInfo))
            //     return Unauthorized("Invalid or expired session");

            if (tokenInfo == null)
            {
                return Unauthorized("Invalid or expired session");
            }
            if (DateTime.UtcNow > tokenInfo.ExpiresAt.ToUniversalTime())

                return Unauthorized("Reddit access token expired");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SubBrowse");

            var response = await httpClient.GetAsync("https://oauth.reddit.com/api/v1/me");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Reddit API error: {response.StatusCode}");
                return StatusCode((int)response.StatusCode, "Reddit API call failed");
            }
            var json = await response.Content.ReadAsStringAsync();
            var redditUser = JsonSerializer.Deserialize<RedditUserDto>(json);
         
            return Ok(new { user = redditUser });
          
           
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var jwt = Request.Cookies["token"]; ;

            Response.Cookies.Append("token", jwt, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1), // expire immediately
                Secure = false,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new { message = "Logged out successfully" });
        }


    }
}
